
using System;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;

public class AgentScript : Agent
{
    public enum Team
    {
        Hider = 0,
        Seeker = 1
    }

    private const int NUM_TEAMS = (int) Team.Seeker;
    
    public TrainingAreaScript area;

    private Rigidbody rBody;
    private FixedJoint fJoint;
    private ViewField vf;
    private MovableScript _carryingMovable = null;
    
    private BufferSensorComponent _bufferSensor;
    
    private float _forceMultiplier;
    private float _rotationMultiplier;
    
    BehaviorParameters behaviorParameters;
    
    [HideInInspector]
    public Team team;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        fJoint = GetComponent<FixedJoint>();
        vf = GetComponent<ViewField>();
        _bufferSensor = GetComponent<BufferSensorComponent>();
        behaviorParameters = GetComponent<BehaviorParameters>();

        team = behaviorParameters.TeamId == (int) Team.Hider ? Team.Hider : Team.Seeker;

        MaxStep = area.episodeLength;
        _forceMultiplier = area.agentsForceMultiplier;
        _rotationMultiplier = area.agentsRotationMultiplier;
        area.agents.Add(this);
    }

    private bool IsNoCollision()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, rBody.transform.localScale.x - 0.001f);

        return colliders.Length == 0;
    }
    
    public override void OnEpisodeBegin()
    {
        if (_carryingMovable != null)
            _carryingMovable.Drop();
        _carryingMovable = null;
        if (fJoint != null)
            Destroy(fJoint);
        fJoint = null;
        //fJoint.connectedBody = dummyJoint;
        
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        
        transform.Rotate(0,Random.value * 360 - 180f, 0);

        int attempts = area.maxRespawnAttempts;
        while (area.maxRespawnAttempts == attempts || !IsNoCollision() && attempts > 0)
        {
            attempts--;
            if (team == Team.Hider)
            {
                transform.localPosition = area.GetVectorInsideRoom();
            }
            else
            {
                transform.localPosition = area.GetVectorOutsideRoom();
            }
            Physics.SyncTransforms();
        }
        Physics.SyncTransforms();
        //Debug.Log(attempts);
        area.preparedAgents++;
        area.ResetEnv();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log(this.GetCumulativeReward());

        // Agent position
        var position = transform.localPosition;
        sensor.AddObservation(position.x);
        sensor.AddObservation(position.z);
        // Agent velocity
        var selfVelocity = rBody.velocity;
        sensor.AddObservation(selfVelocity.x);
        sensor.AddObservation(selfVelocity.z);
        // Time left for preparation
        if (StepCount > area.preparingPhaseLength)
            sensor.AddObservation(0.0f);
        else
            sensor.AddObservation(area.preparingPhaseLength - StepCount);
        
        foreach (GameObject obj in vf.collectVisibleObjects())
        {
            if (!obj.CompareTag("Hider") && !obj.CompareTag("Seeker") && !obj.CompareTag("Cube")) continue;
            //Debug.Log(obj.tag);
            if (this.team == Team.Seeker && obj.CompareTag("Hider"))
            {
                area.isAnyHiderSeen = true;
            }

            float[] observations = new float[7];
            Rigidbody objectRigidBody = obj.GetComponent<Rigidbody>();
            // Other object position
            var localPosition = obj.transform.localPosition;
            observations[0] = localPosition.x / 10.0f;
            observations[1] = localPosition.z / 10.0f;
            // Other object velocity
            var velocity = objectRigidBody.velocity;
            observations[2] = velocity.x / 10.0f;
            observations[3] = velocity.z / 10.0f;
            // Other object team
            if (obj.CompareTag("Hider"))
                observations[4] = 1.0f;
            else if (obj.CompareTag("Seeker"))
                observations[5] = 1.0f;
            else
                observations[6] = 1.0f;
            _bufferSensor.AppendObservation(observations);
        }
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log(this.StepCount);
        if (team == Team.Seeker && StepCount <= area.preparingPhaseLength)
            return;
        
        Vector3 controlSignal = Vector3.zero;
        
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        //Debug.Log(controlSignal);
        rBody.AddForce(controlSignal * _forceMultiplier);
        rBody.AddTorque(transform.up * _rotationMultiplier * actions.ContinuousActions[2]);

        Vector3 lPos = transform.localPosition; 
        if (lPos.x < -10.0 || lPos.x > 10.0 || lPos.z < -10.0 || lPos.z > 10.0)
            AddReward(area.penaltyForLeaving * area.rewardScale);
        //Debug.Log("Reward:" + this.GetCumulativeReward().ToString() + team.ToString());

        /* Discrete actions mapping:
         *   0 -- no action
         *   1 -- drop box
         *   2 -- grab box
         *   3 -- unlock box
         *   4 -- lock box
         */
        int action = actions.DiscreteActions[0];
        //Debug.Log(action);
        if (_carryingMovable != null && action == 1)
        {
            _carryingMovable.Drop();
            _carryingMovable = null;
            Destroy(fJoint);
            fJoint = null;
        }
        else if (_carryingMovable == null)
        {
            GameObject nearestCube = null;
            float minDistToCube = Single.MaxValue;
            
            foreach (GameObject obj in vf.collectVisibleObjects())
            {
                if (!obj.CompareTag("Cube")) continue;

                float dist = Vector3.Distance(transform.localPosition, obj.transform.localPosition);
                if (dist < minDistToCube)
                {
                    nearestCube = obj;
                    minDistToCube = dist;
                }
            }
            
            // Check if agent see box nearby
            if (nearestCube == null || area.maxDistToInteractWithBox < minDistToCube)
                return;

            MovableScript movableScript = nearestCube.GetComponent<MovableScript>();
            if (action == 2)
            {
                if (!movableScript.Grab(rBody))
                    return;
                _carryingMovable = movableScript;
                fJoint = gameObject.AddComponent<FixedJoint>();
                fJoint.connectedBody = nearestCube.GetComponent<Rigidbody>();
            }
            else if (action == 3)
            {
                movableScript.Unlock(team);
            }
            else if (action == 4)
            {
                movableScript.Lock(team);
            }
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //Debug.Log("Heuristic called");
        var continiousActionsOut = actionsOut.ContinuousActions;
        continiousActionsOut[0] = Input.GetAxis("Horizontal");
        continiousActionsOut[1] = Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.Q))
            continiousActionsOut[2] = -1.0f;
        else if (Input.GetKey(KeyCode.E))
            continiousActionsOut[2] = 1.0f;
        else
            continiousActionsOut[2] = 0.0f;

        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.Alpha1))
            discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.Alpha2))
            discreteActionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.Alpha3))
            discreteActionsOut[0] = 3;
        else if (Input.GetKey(KeyCode.Alpha4))
            discreteActionsOut[0] = 4;
        else
            discreteActionsOut[0] = 0;
    }
}
