using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class TrainingAreaScript : MonoBehaviour
{
    private List<AgentScript> _agents;
    private ObjectsManager _manager;
    private Config _config;
    private List<MovableScript> _movables;
    private WallsSpawner _wallsSpawner;
    
    void Awake()
    {
        Academy.Instance.OnEnvironmentReset += ResetEnv;
        Academy.Instance.AgentPreStep += PreStep;
        _manager = transform.Find("ManagerObject").GetComponent<ObjectsManager>();
    }
    
    public void ResetEnv()
    {
        _config = _manager.GetConfig();
        _wallsSpawner = _manager.GetWallsSpawner();
        _agents = _manager.GetAgents();
        _movables = _manager.GetMovables();

        if (!_agents.All(agent => agent.GetIsPrepared()))
            return;

        // Randomize holes in the room walls
        _wallsSpawner.Respawn();
        
        // Place boxes and ramp
        foreach (var movable in _movables)
        {
            movable.Respawn();
        }
    }

    void PreStep(int i)
    {
        var hidersReward = _manager.GetSeenHolder().isAnyHiderSeen ? -1.0f : 1.0f;
        hidersReward *= _config.rewardScale;
        
        foreach (var agent in _agents.TakeWhile(agent => agent.StepCount >= _config.preparingPhaseLength))
        {
            if (agent.team == AgentScript.Team.Hider)
                agent.AddReward(hidersReward);
            else
                agent.AddReward(-hidersReward);
        }
        //Debug.Log("IsSeen:" + _manager.GetSeenHolder().isAnyHiderSeen + i);
        _manager.GetSeenHolder().isAnyHiderSeen = false;
    }
    
    private void Update()
    {
        ResetEnv();
    }
}
