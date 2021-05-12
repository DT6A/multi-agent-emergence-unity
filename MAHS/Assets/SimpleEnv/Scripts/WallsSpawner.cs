using System.Collections.Generic;
using UnityEngine;

public class WallsSpawner : MonoBehaviour
{
    private ObjectsManager _manager;
    private Config _config;
    private List<Transform> _walls;

    // Start is called before the first frame update
    void Start()
    {
        _manager = transform.parent.Find("ManagerObject").GetComponent<ObjectsManager>();
        _config = _manager.GetConfig();
        _walls = _manager.GetWalls();
    }

    public void Respawn()
    {
        if (_walls == null)
        {
            _walls = _manager.GetWalls();
        }

        if (_config == null)
        {
            _config = _manager.GetConfig();
        }
        
        // First pair of walls
        var transform1 = _walls[0];
        Vector3 localPosition1;
        localPosition1 = new Vector3(
            0.1f,
            0.75f,
            5.0f - Random.value * (5.0f - _config.roomHolesSize) / 2
        );
        transform1.localPosition = localPosition1;

        Vector3 localScale1;
        localScale1 = new Vector3(
            (5.0f - localPosition1.z) * 2,
            1.5f,
            0.2f
        );
        transform1.localScale = localScale1;
        
        var transform2 = _walls[1];
        Vector3 localPosition2;
        localPosition2 = new Vector3(
            0.1f,
            0.75f,
            (5.0f - _config.roomHolesSize / 2 - localScale1.x) / 2
        );
        transform2.localPosition = localPosition2;

        Vector3 localScale2;
        localScale2 = new Vector3(
            localPosition2.z * 2,
            1.5f,
            0.2f
        );
        transform2.localScale = localScale2;
          
        // Second pair of walls
        var transform3 = _walls[2];
        Vector3 localPosition;
        localPosition = new Vector3(
            5.0f - Random.value * (5.0f - _config.roomHolesSize) / 2,
            0.75f,
            -0.1f
            );
        transform3.localPosition = localPosition;

        Vector3 localScale3;
        localScale3 = new Vector3(
            (5.0f - localPosition.x) * 2,
            1.5f,
            0.2f
            );
        transform3.localScale = localScale3;
        
        var transform4 = _walls[3];
        Vector3 localPosition4;
        localPosition4 = new Vector3(
            (5.0f - _config.roomHolesSize / 2 - localScale3.x) / 2,
            0.75f,
            -0.1f
        );
        transform4.localPosition = localPosition4;

        Vector3 localScale4;
        localScale4 = new Vector3(
            localPosition4.x * 2,
            1.5f,
            0.2f
        );
        transform4.localScale = localScale4;
    }
}
