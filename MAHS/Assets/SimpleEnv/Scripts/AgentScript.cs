using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Debug = UnityEngine.Debug;

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
    BehaviorParameters behaviorParameters;
    [HideInInspector]
    public Team team;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        vf = GetComponent<ViewField>();
        behaviorParameters = GetComponent<BehaviorParameters>();
        
        if (behaviorParameters.TeamId == (int) Team.Hider)
        {
            team = Team.Hider;
        }
        else
        {
            team = Team.Seeker; 
        }

        this.MaxStep = area.episodeLength;
        
        area.agents.Add(this);
    }
    
    public override void OnEpisodeBegin()
    {
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.Rotate(0,Random.value * 360 - 180f, 0);
        
        if (team == Team.Hider)
        {
            this.transform.localPosition = new Vector3(2 * (Random.value * 4.25f + 0.25f), 0.5f, 2 * (-Random.value * 4.25f - 0.25f));
        }
        else
        {
            this.transform.localPosition = new Vector3(2 * (-Random.value * 5f + 0.5f), 0.5f, 2 * (Random.value * 9f - 4.5f));
        }
        
        area.ResetEnv();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log(this.GetCumulativeReward());
        // Agent role
        sensor.AddOneHotObservation((int)team, NUM_TEAMS);
        
        // Agent position
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

        Quaternion opponentInfo = new Quaternion(0, 0, 0, 0);
        foreach (GameObject obj in vf.collectVisibleObjects())
        {
            if (obj.CompareTag("Hider") || obj.CompareTag("Seeker"))
            {
                if (team == Team.Seeker)
                {
                    area.isAnyHiderSeen = true;
                }

                Rigidbody otherAgentRigidbody = obj.GetComponent<Rigidbody>();
                // Opponent position
                opponentInfo.x =  obj.transform.localPosition.x;
                opponentInfo.y =  obj.transform.localPosition.z;
                // Opponent velocity
                opponentInfo.z = otherAgentRigidbody.velocity.x;
                opponentInfo.w = otherAgentRigidbody.velocity.z;
                break;
            }
        }
        
        sensor.AddObservation(opponentInfo.x);
        sensor.AddObservation(opponentInfo.y);
        sensor.AddObservation(opponentInfo.z);
        sensor.AddObservation(opponentInfo.w);
    }
    
    public float forceMultiplier = 10.0f;
    public float rotationMultiplier = 1.0f;
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log(this.StepCount);
        if (team == Team.Seeker && this.StepCount <= area.preparingPhaseLength)
            return;
        
        Vector3 controlSignal = Vector3.zero;
        
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        //Debug.Log(controlSignal);
        rBody.AddForce(controlSignal * forceMultiplier);
        rBody.AddTorque(transform.up * rotationMultiplier * actions.ContinuousActions[2]);
        
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
