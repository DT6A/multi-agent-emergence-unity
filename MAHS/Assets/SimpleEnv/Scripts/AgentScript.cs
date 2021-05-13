using System;
using System.Linq;
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

    private Rigidbody _rBody;
    private FixedJoint _fJoint;
    private ViewField _viewField;
    private MovableScript _carryingMovable;
    private BehaviorParameters _behaviorParameters;
    private BufferSensorComponent _bufferSensor;
    private ObjectsManager _manager;
    private Config _config;
    private SpawnHelper _spawnHelper;
    private SeenHolder _seenHolder;

    private bool _isPrepared;

    public String seekerTag = "Seeker";
    public String hiderTag = "Hider";
    public String cubeTag = "Cube";
    public String rampTag = "Ramp";
    
    [HideInInspector]
    public Team team;
    void Start()
    {
        _rBody = GetComponent<Rigidbody>();
        _fJoint = GetComponent<FixedJoint>();
        _viewField = GetComponent<ViewField>();
        _bufferSensor = GetComponent<BufferSensorComponent>();
        _behaviorParameters = GetComponent<BehaviorParameters>();
        
        var parent = transform.parent;
        _manager = parent.GetComponent<ObjectsManager>();
        _config = parent.GetComponent<Config>();
        _spawnHelper = parent.GetComponent<SpawnHelper>();
        _seenHolder = parent.GetComponent<SeenHolder>();

        team = _behaviorParameters.TeamId == (int) Team.Hider ? Team.Hider : Team.Seeker;

        MaxStep = _config.episodeLength;
    }

    public bool GetIsPrepared()
    {
        return _isPrepared;
    }

    public override void OnEpisodeBegin()
    {
        _isPrepared = false;
        if (_carryingMovable != null)
            _carryingMovable.Drop();
        _carryingMovable = null;
        if (_fJoint != null)
            Destroy(_fJoint);
        _fJoint = null;
        //fJoint.connectedBody = dummyJoint;
        
        _rBody.angularVelocity = Vector3.zero;
        _rBody.velocity = Vector3.zero;
        
        transform.Rotate(0,Random.value * 360 - 180f, 0);

        int attempts = _config.maxRespawnAttempts;
        while (_config.maxRespawnAttempts == attempts || _spawnHelper.AnyCollisionDetected(transform) && attempts > 0)
        {
            attempts--;
            transform.localPosition = team == Team.Hider
                ? _spawnHelper.GetVectorInsideRoom()
                : _spawnHelper.GetVectorOutsideRoom();
            Physics.SyncTransforms();
        }
        Physics.SyncTransforms();
        //Debug.Log(attempts);
        _isPrepared = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        _isPrepared = false;
        //Debug.Log(this.GetCumulativeReward());

        // Agent position
        var position = transform.localPosition;
        sensor.AddObservation(position.x);
        sensor.AddObservation(position.z);
        // Agent velocity
        var selfVelocity = _rBody.velocity;
        sensor.AddObservation(selfVelocity.x);
        sensor.AddObservation(selfVelocity.z);
        // Time left for preparation
        if (StepCount > _config.preparingPhaseLength)
            sensor.AddObservation(0.0f);
        else
            sensor.AddObservation(_config.preparingPhaseLength - StepCount);
        
        foreach (var obj in _viewField.collectVisibleObjects().Where(obj =>
            obj.CompareTag(hiderTag) || obj.CompareTag(seekerTag) || obj.CompareTag(cubeTag) || obj.CompareTag(rampTag)))
        {
            //Debug.Log(obj.tag);
            if (team == Team.Seeker && obj.CompareTag(hiderTag))
            {
                _seenHolder.isAnyHiderSeen = true;
            }

            float[] observations = new float[8];
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
            if (obj.CompareTag(hiderTag))
                observations[4] = 1.0f;
            else if (obj.CompareTag(seekerTag))
                observations[5] = 1.0f;
            else if (obj.CompareTag(cubeTag))
                observations[6] = 1.0f;
            else
                observations[7] = 1.0f;
            _bufferSensor.AppendObservation(observations);
        }
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log(this.StepCount);
        if (team == Team.Seeker && StepCount <= _config.preparingPhaseLength)
            return;
        
        Vector3 controlSignal = Vector3.zero;
        
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        //Debug.Log(controlSignal);
        _rBody.AddForce(controlSignal * _config.agentsForceMultiplier);
        _rBody.AddTorque(transform.up * _config.agentsRotationMultiplier * actions.ContinuousActions[2]);

        Vector3 lPos = transform.localPosition; 
        if (lPos.x < -10.0 || lPos.x > 10.0 || lPos.z < -10.0 || lPos.z > 10.0)
            AddReward(_config.penaltyForLeaving * _config.rewardScale);
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
            Destroy(_fJoint);
            _fJoint = null;
        }
        else if (_carryingMovable == null)
        {
            GameObject nearestCube = null;
            float minDistToCube = Single.MaxValue;
            
            foreach (GameObject obj in _viewField.collectVisibleObjects())
            {
                if (!obj.CompareTag(cubeTag) && !obj.CompareTag(rampTag)) continue;

                float dist = Vector3.Distance(transform.localPosition, obj.transform.localPosition);
                if (dist < minDistToCube)
                {
                    nearestCube = obj;
                    minDistToCube = dist;
                }
            }
            
            // Check if agent see box nearby
            if (nearestCube == null || _config.maxDistToInteractWithBox < minDistToCube)
                return;

            MovableScript movableScript = nearestCube.GetComponent<MovableScript>();
            
            switch (action)
            {
                case 2:
                {
                    if (!movableScript.Grab(_rBody))
                        return;
                    _carryingMovable = movableScript;
                    _fJoint = gameObject.AddComponent<FixedJoint>();
                    _fJoint.connectedBody = nearestCube.GetComponent<Rigidbody>();
                    break;
                }
                case 3:
                {
                    movableScript.Unlock(team);
                    break;
                }
                case 4:
                {
                    movableScript.Lock(team);
                    break;
                }       
                default:
                    return;
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
