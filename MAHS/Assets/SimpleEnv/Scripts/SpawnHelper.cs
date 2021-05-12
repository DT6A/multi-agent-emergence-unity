using System.Linq;
using UnityEngine;

public class SpawnHelper : MonoBehaviour
{
    private ObjectsManager _manager;
    private Config _config;

    // Start is called before the first frame update
    void Start()
    {
        _manager = transform.parent.Find("ManagerObject").GetComponent<ObjectsManager>();
        _config = _manager.GetConfig();  
    }
    
    public Vector3 GetVectorInsideRoom()
    {
        if (_config == null)
        {
            _config = _manager.GetConfig();
        }
        return new Vector3(2 * (Random.value * _config.spawnInsideRoomWidth + _config.spawnInsideRoomOffset), 0.8f,
            2 * (Random.value * _config.spawnInsideRoomWidth + _config.spawnInsideRoomOffset));
    }
    
    public Vector3 GetVectorOutsideRoom()
    {
        if (_config == null)
        {
            _config = _manager.GetConfig();
        }
        Vector3 pos = new Vector3(2 * (Random.value * _config.spawnOutsideRoomWidth - _config.spawnOutsideRoomOffset),
            0.8f,
            2 * (Random.value * _config.spawnOutsideRoomWidth - _config.spawnOutsideRoomOffset));
        while (pos.x > -1.0 && pos.z > -1.0)
            pos = new Vector3(2 * (Random.value * _config.spawnOutsideRoomWidth - _config.spawnOutsideRoomOffset), 0.8f,
                2 * (Random.value * _config.spawnOutsideRoomWidth - _config.spawnOutsideRoomOffset));
        return pos;
    }
    
    public bool AnyCollisionDetected(Transform objectTransform)
    {
        if (_manager.GetAgents().Any(agent => CollisionBetweenTwo(objectTransform, agent.transform)))
        {
            return true;
        }

        if (_manager.GetMovables().Any(movable => CollisionBetweenTwo(objectTransform, movable.transform)))
        {
            return true;
        }
        
        return false;
    }
    
    public bool CollisionBetweenTwo(Transform objectTransform, Transform otherTransform)
    {
        if (_config == null)
        {
            _config = _manager.GetConfig();
        }
        return !otherTransform.transform.Equals(objectTransform) &&
               Vector3.Distance(objectTransform.localPosition, otherTransform.localPosition) <= _config.intersectionDistance;
    }
}
