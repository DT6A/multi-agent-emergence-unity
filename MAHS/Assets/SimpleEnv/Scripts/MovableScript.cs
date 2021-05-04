using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MovableScript : MonoBehaviour
{
    public TrainingAreaScript area;
    
    public Color defaultColor = new Color(1.0f, 0.7960784f, 0.1529412f);
    public Color hiderColor = new Color(33.0f / 255.0f, 150.0f / 255.0f, 243.0f / 255.0f);
    public Color seekerColor = new Color(141.0f / 255.0f, 109.0f / 255.0f, 200.0f / 255.0f);

    public enum ObjectType
    {
        Box = 0,
        Ramp = 1
    }

    public ObjectType objectType;
   
    private Material material;
    private Rigidbody rBody;

    private bool isLocked;
    private bool isGrabed;
    private AgentScript.Team? teamLocked;
    
    private void ResetLock()
    {
        isLocked = false;
        teamLocked = null;
        material.color = defaultColor;
        rBody.isKinematic = false;
    }

    public void Lock(AgentScript.Team team)
    {
        if (isLocked || isGrabed) return;
        
        isLocked = true;
        teamLocked = team;

        material.color = team == AgentScript.Team.Hider ? hiderColor : seekerColor;
        rBody.isKinematic = true;
    }
    
    public void Unlock(AgentScript.Team team)
    {
        if (isLocked && team == teamLocked)
        {
            ResetLock();
        }
    }

    public bool Grab(Rigidbody agentRigidbody)
    {
        if (!isLocked && !isGrabed)
        {
            //rBody.constraints = RigidbodyConstraints.None;
            rBody.mass = 0.02f;
            isGrabed = true;
            return true;
        }

        return false;
    }

    public void Drop()
    {
        //rBody.constraints = objectType == ObjectType.Box ? boxConstraints : rampConstraints;
        isGrabed = false;
        rBody.mass = 0.5f;
    }
    
    public void Respawn()
    {
        ResetLock();
        Drop();
        transform.localPosition = objectType == ObjectType.Box ?
            area.GetVectorInsideRoom() :
            area.GetVectorOutsideRoom();

        if (objectType == ObjectType.Box)
        {
            transform.rotation = Quaternion.identity;
            transform.Rotate(0, Random.value * 360 - 180f, 0);
        }
        else
        {
            transform.rotation = Quaternion.AngleAxis(-90, new Vector3(1, 0, 0));
            transform.Rotate(0, 0, Random.value * 360 - 180f);
        }

        Physics.SyncTransforms();
        int attempts = area.maxRespawnAttempts;
        while (area.AnyCollisionDetected(transform) && attempts > 0)
        {
            transform.localPosition = objectType == ObjectType.Box ? 
                area.GetVectorInsideRoom() :
                area.GetVectorOutsideRoom();
            if (objectType == ObjectType.Box)
                transform.Rotate(0, Random.value * 360 - 180f, 0);
            else
                transform.Rotate(0, 0, Random.value * 360 - 180f);
            Physics.SyncTransforms();
            attempts--;
        }
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        Physics.SyncTransforms();
        //Debug.Log(attempts);
        //isSpawnNeeded = false;
    }
    // Start is called before the first frame update
    
    void Start()
    {
        rBody = GetComponentInChildren<Rigidbody>();
        material = GetComponentInChildren<MeshRenderer>().material;
        Respawn();
    }

    // Update is called once per frame
    void Update()
    {
        // For debug
        if (Input.GetKey(KeyCode.U))
            Unlock(AgentScript.Team.Hider);
        if (Input.GetKey(KeyCode.I))
            Lock(AgentScript.Team.Hider);
        if (Input.GetKey(KeyCode.O))
            Unlock(AgentScript.Team.Seeker);
        if (Input.GetKey(KeyCode.P))
            Lock(AgentScript.Team.Seeker);
    }

    private void FixedUpdate()
    {
        /*
        if (IsNeedSpawn)
        {
            Respawn();
        }
        */
    }
}
