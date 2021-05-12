using System.Collections.Generic;
using UnityEngine;

public class ObjectsManager : MonoBehaviour
{
    private Config _config;
    private WallsSpawner _wallsSpawner;
    private SpawnHelper _spawnHelper;
    private SeenHolder _seenHolder;
    private List<AgentScript> _agents = new List<AgentScript>();
    private List<MovableScript> _movables = new List<MovableScript>();
    private List<Transform> _roomWalls = new List<Transform>();
    
    // Start is called before the first frame update
    void Start()
    {
        var parent = transform.parent;
        
        _config = parent.Find("ConfigObject").GetComponent<Config>();
        _wallsSpawner = parent.Find("WallsSpawner").GetComponent<WallsSpawner>();
        _spawnHelper = parent.Find("SpawnHelper").GetComponent<SpawnHelper>();
        _seenHolder = parent.Find("SeenStateHolder").GetComponent<SeenHolder>();
        
        foreach (var agent in FindObjectsOfType(typeof(AgentScript)))
        {
            _agents.Add((AgentScript) agent);
        }
        
        foreach (var movable in FindObjectsOfType(typeof(MovableScript)))
        {
            _movables.Add((MovableScript) movable);
        }

        foreach (Transform wall in parent.Find("Walls").Find("Room"))
        {
            _roomWalls.Add(wall);
        }
    }

    public Config GetConfig()
    {
        return _config;
    }

    public WallsSpawner GetWallsSpawner()
    {
        return _wallsSpawner;
    }
    
    public SpawnHelper GetSpawnHelper()
    {
        return _spawnHelper;
    }

    public SeenHolder GetSeenHolder()
    {
        return _seenHolder;
    }
    
    public List<AgentScript> GetAgents()
    {
        return _agents;
    }
    
    public List<MovableScript> GetMovables()
    {
        return _movables;
    }

    public List<Transform> GetWalls()
    {
        return _roomWalls;
    }
}
