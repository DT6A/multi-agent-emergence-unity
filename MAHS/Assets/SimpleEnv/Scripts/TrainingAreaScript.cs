using System;
using System.Collections;
using System.Collections.Generic;

using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrainingAreaScript : MonoBehaviour
{
    [HideInInspector]
    public bool isAnyHiderSeen;
    [HideInInspector]
    public List<AgentScript> agents;
    void Awake()
    {
        Academy.Instance.OnEnvironmentReset += ResetEnv;
        Academy.Instance.AgentPreStep += PreStep;
    }

    public int episodeLength = 1000;
    public int preparingPhaseLength = 400;
    
    public Transform box1;
    public Transform box2;
    public void ResetEnv()
    {
        box1.transform.localPosition = new Vector3(Random.value * 4.25f + 0.25f, 0.5f, -Random.value * 4.25f - 0.25f);
        box2.transform.localPosition = new Vector3(Random.value * 4.25f + 0.25f, 0.5f, -Random.value * 4.25f - 0.25f);
    }

    void PreStep(int i)
    {
        float hidersReward = 1.0f;
        if (isAnyHiderSeen)
            hidersReward = -1.0f;
        
        foreach (AgentScript agent in agents)
        {
            if (agent.StepCount < preparingPhaseLength)
                break;
            if (agent.team == AgentScript.Team.Hider)
                agent.AddReward(hidersReward);
            else
                agent.AddReward(-hidersReward);
        }
        //Debug.Log("IsSeen:" + isAnyHiderSeen + i);
        isAnyHiderSeen = false;
    }

}
