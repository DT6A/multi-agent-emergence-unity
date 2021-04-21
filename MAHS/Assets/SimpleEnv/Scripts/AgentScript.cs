
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

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
    private ViewField vf;
    private BufferSensorComponent bufferSensor;
    private float forceMultiplier;
    private float rotationMultiplier;
    BehaviorParameters behaviorParameters;
    [HideInInspector]
    public Team team;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        vf = GetComponent<ViewField>();
        bufferSensor = GetComponent<BufferSensorComponent>();
        behaviorParameters = GetComponent<BehaviorParameters>();

        team = behaviorParameters.TeamId == (int) Team.Hider ? Team.Hider : Team.Seeker;

        MaxStep = area.episodeLength;
        forceMultiplier = area.agentsForceMultiplier;
        rotationMultiplier = area.agentsRotationMultiplier;
        area.agents.Add(this);
    }
    
    public override void OnEpisodeBegin()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.Rotate(0,Random.value * 360 - 180f, 0);
        
        if (team == Team.Hider)
        {
            transform.localPosition =
                new Vector3(2 * (Random.value * area.spawnInsideBoxWidth + area.spawnInsideBoxOffset), 0.5f,
                    2 * (-Random.value * area.spawnInsideBoxWidth - area.spawnInsideBoxOffset));
        }
        else
        {
            Vector3 pos = new Vector3(2 * (Random.value * area.spawnOutsideBoxWidth - area.spawnOutsideBoxOffset), 0.5f,
                2 * (Random.value * area.spawnOutsideBoxWidth - area.spawnOutsideBoxOffset));
            while (pos.x > 0 && pos.z < 0)
                pos = new Vector3(2 * (Random.value * area.spawnOutsideBoxWidth - area.spawnOutsideBoxOffset), 0.5f,
                    2 * (Random.value * area.spawnOutsideBoxWidth - area.spawnOutsideBoxOffset));
            transform.localPosition = pos;
        }
        
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

        foreach (GameObject obj in vf.collectVisibleObjects())
        {
            if (!obj.CompareTag("Hider") && !obj.CompareTag("Seeker")) continue;
            //Debug.Log(obj.tag);
            if (this.team == Team.Seeker && obj.CompareTag("Hider"))
            {
                area.isAnyHiderSeen = true;
            }

            float[] observations = new float[5];
            Rigidbody otherAgentRigidbody = obj.GetComponent<Rigidbody>();
            // Other agent position
            var localPosition = obj.transform.localPosition;
            observations[0] = localPosition.x / 10.0f;
            observations[1] = localPosition.z / 10.0f;
            // Other agent velocity
            var velocity = otherAgentRigidbody.velocity;
            observations[2] = velocity.x / 10.0f;
            observations[3] = velocity.z / 10.0f;
            // Other agent team
            if (obj.CompareTag("Hider"))
                observations[4] = (float)Team.Hider;
            else
                observations[4] = (float)Team.Seeker;
            bufferSensor.AppendObservation(observations);
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
        rBody.AddForce(controlSignal * forceMultiplier);
        rBody.AddTorque(transform.up * rotationMultiplier * actions.ContinuousActions[2]);

        Vector3 lPos = transform.localPosition; 
        if (lPos.x < -10.0 || lPos.x > 10.0 || lPos.z < -10.0 || lPos.z > 10.0)
            AddReward(area.penaltyForLeaving * area.rewardScale);
        //Debug.Log("Reward:" + this.GetCumulativeReward().ToString() + team.ToString());
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
    }
}
