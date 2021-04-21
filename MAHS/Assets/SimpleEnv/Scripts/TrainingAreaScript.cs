using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrainingAreaScript : MonoBehaviour
{
    [HideInInspector]
    public bool isAnyHiderSeen;
    [HideInInspector]
    public List<AgentScript> agents;

    public float agentsForceMultiplier = 50.0f;
    public float agentsRotationMultiplier = 3.0f;

    public float spawnInsideBoxWidth = 4.25f;
    public float spawnInsideBoxOffset = 0.25f;
    public float spawnOutsideBoxWidth = 9f;
    public float spawnOutsideBoxOffset = 4.5f;

    public float rewardScale = 0.1f;
    public float penaltyForLeaving = -5.0f;
    
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
        box1.transform.localPosition = new Vector3(Random.value * spawnInsideBoxWidth + spawnInsideBoxOffset, 0.5f,
            -Random.value * spawnInsideBoxWidth - spawnInsideBoxOffset);
        box2.transform.localPosition = new Vector3(Random.value * spawnInsideBoxWidth + spawnInsideBoxOffset, 0.5f,
            -Random.value * spawnInsideBoxWidth - spawnInsideBoxOffset);
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

}
