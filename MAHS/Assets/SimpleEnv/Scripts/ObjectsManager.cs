using System.Collections.Generic;
using UnityEngine;

public class ObjectsManager : MonoBehaviour
{
    private List<AgentScript> _agents = new List<AgentScript>();
    private List<MovableScript> _movables = new List<MovableScript>();
    private List<Transform> _roomWalls = new List<Transform>();
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var agent in GetComponentsInChildren(typeof(AgentScript)))
        {
            _agents.Add((AgentScript) agent);
        }
        
        foreach (var movable in GetComponentsInChildren(typeof(MovableScript)))
        {
            _movables.Add((MovableScript) movable);
        }

        foreach (Transform wall in transform.Find("Walls").Find("Room"))
        {
            _roomWalls.Add(wall);
        }
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
