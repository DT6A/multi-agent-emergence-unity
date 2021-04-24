using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TrainingAreaScript : MonoBehaviour
{
    [HideInInspector]
    public bool isAnyHiderSeen;
    [HideInInspector]
    public List<AgentScript> agents;

    public int preparedAgents = 0;

    public float agentsForceMultiplier = 50.0f;
    public float agentsRotationMultiplier = 3.0f;

    [FormerlySerializedAs("spawnInsideBoxWidth")] public float spawnInsideRoomWidth = 4.25f;
    [FormerlySerializedAs("spawnInsideBoxOffset")] public float spawnInsideRoomOffset = 0.25f;
    [FormerlySerializedAs("spawnOutsideBoxWidth")] public float spawnOutsideRoomWidth = 9f;
    [FormerlySerializedAs("spawnOutsideBoxOffset")] public float spawnOutsideRoomOffset = 4.5f;

    public float rewardScale = 0.1f;
    public float penaltyForLeaving = -5.0f;

    public float maxDistToInteractWithBox = 2f;
    void Awake()
    {
        Academy.Instance.OnEnvironmentReset += ResetEnv;
        Academy.Instance.AgentPreStep += PreStep;
    }

    public int episodeLength = 1000;
    public int preparingPhaseLength = 400;

    public float roomHolesSize = 1.7f;
    
    public MovableScript box1;
    public MovableScript box2;
    public MovableScript ramp;
    
    public Transform wall1;
    public Transform wall2;
    
    public Transform wall3;
    public Transform wall4;
    
    public int maxRespawnAttempts = 10;
    public void ResetEnv()
    {
        if (preparedAgents != agents.Count)
            return;
        preparedAgents = 0;
        
        
        // Randomize holes in the room walls
        // First pair of walls
        var transform1 = wall1.transform;
        Vector3 localPosition1 = transform1.localPosition;
        localPosition1 = new Vector3(
            localPosition1.x,
            localPosition1.y,
            5.0f - Random.value * (5.0f - roomHolesSize) / 2
        );
        transform1.localPosition = localPosition1;

        var localScale1 = transform1.localScale;
        localScale1 = new Vector3(
            (5.0f - localPosition1.z) * 2,
            localScale1.y,
            localScale1.z
        );
        transform1.localScale = localScale1;
        
        var transform2 = wall2.transform;
        Vector3 localPosition2 = transform2.localPosition;
        localPosition2 = new Vector3(
            localPosition2.x,
            localPosition2.y,
            (5.0f - roomHolesSize / 2 - localScale1.x) / 2
        );
        //Debug.Log(localPosition2);
        transform2.localPosition = localPosition2;

        var localScale2 = transform2.localScale;
        localScale2 = new Vector3(
            localPosition2.z * 2,
            localScale2.y,
            localScale2.z
        );
        transform2.localScale = localScale2;
          
        // Second pair of walls
        var transform3 = wall3.transform;
        Vector3 localPosition = transform3.localPosition;
        localPosition = new Vector3(
            5.0f - Random.value * (5.0f - roomHolesSize) / 2,
            localPosition.y,
            localPosition.z
            );
        transform3.localPosition = localPosition;

        var localScale3 = transform3.localScale;
        localScale3 = new Vector3(
            (5.0f - localPosition.x) * 2,
            localScale3.y,
            localScale3.z
            );
        transform3.localScale = localScale3;
        
        var transform4 = wall4.transform;
        Vector3 localPosition4 = transform4.localPosition;
        localPosition4 = new Vector3(
            (5.0f - roomHolesSize / 2 - localScale3.x) / 2,
            localPosition4.y,
            localPosition4.z
        );
        transform4.localPosition = localPosition4;

        var localScale4 = transform4.localScale;
        localScale4 = new Vector3(
            localPosition4.x * 2,
            localScale4.y,
            localScale4.z
        );
        transform4.localScale = localScale4;
        
        // Place boxes and ramp
        box1.Respawn();
        box2.Respawn();
        ramp.Respawn();
    }

    void PreStep(int i)
    {
        var hidersReward = isAnyHiderSeen ? -1.0f : 1.0f;
        hidersReward *= rewardScale;
        
        foreach (var agent in agents.TakeWhile(agent => agent.StepCount >= preparingPhaseLength))
        {
            if (agent.team == AgentScript.Team.Hider)
                agent.AddReward(hidersReward);
            else
                agent.AddReward(-hidersReward);
        }
        //Debug.Log("IsSeen:" + isAnyHiderSeen + i);
        isAnyHiderSeen = false;
    }

    public Vector3 GetVectorInsideRoom()
    {
        return new Vector3(2 * (Random.value * spawnInsideRoomWidth + spawnInsideRoomOffset), 0.8f,
            2 * (Random.value * spawnInsideRoomWidth + spawnInsideRoomOffset));
    }
    
    public Vector3 GetVectorOutsideRoom()
    {
        Vector3 pos = new Vector3(2 * (Random.value * spawnOutsideRoomWidth - spawnOutsideRoomOffset),
            0.8f,
            2 * (Random.value * spawnOutsideRoomWidth - spawnOutsideRoomOffset));
        while (pos.x > -1.0 && pos.z > -1.0)
            pos = new Vector3(2 * (Random.value * spawnOutsideRoomWidth - spawnOutsideRoomOffset), 0.8f,
                2 * (Random.value * spawnOutsideRoomWidth - spawnOutsideRoomOffset));
        return pos;
    }
}
