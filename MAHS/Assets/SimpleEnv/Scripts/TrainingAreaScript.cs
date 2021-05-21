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
    private SeenHolder _seenHolder;
    
    void Awake()
    {
        Academy.Instance.OnEnvironmentReset += ResetEnv;
        Academy.Instance.AgentPreStep += PreStep;
        
        _manager = GetComponent<ObjectsManager>();
        _config = GetComponent<Config>();
        _wallsSpawner = GetComponent<WallsSpawner>();
        _seenHolder = GetComponent<SeenHolder>();
    }
    
    public void ResetEnv()
    {
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
        var hidersReward = _seenHolder.isAnyHiderSeen ? -1.0f : 1.0f;
        hidersReward *= _config.rewardScale;
        
        foreach (var agent in _agents.TakeWhile(agent => agent.StepCount >= _config.preparingPhaseLength))
        {
            if (agent.team == AgentScript.Team.Hider)
                agent.AddReward(hidersReward);
            else
                agent.AddReward(-hidersReward);
        }
        _seenHolder.isAnyHiderSeen = false;
    }
    
    private void Update()
    {
        ResetEnv();
    }
}
