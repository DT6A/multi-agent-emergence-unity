using UnityEngine;
using Random = UnityEngine.Random;

public class MovableScript : MonoBehaviour
{
    public Color defaultColor = new Color(1.0f, 0.7960784f, 0.1529412f);
    public Color hiderColor = new Color(33.0f / 255.0f, 150.0f / 255.0f, 243.0f / 255.0f);
    public Color seekerColor = new Color(141.0f / 255.0f, 109.0f / 255.0f, 200.0f / 255.0f);

    public enum ObjectType
    {
        Box = 0,
        Ramp = 1
    }

    public ObjectType objectType;
   
    private bool _isLocked;
    private bool _isGrabed;
    private AgentScript.Team? _teamLocked;
    private Material _material;
    private Rigidbody _rBody;
    private ObjectsManager _manager; 
    private Config _config;    
    

    private void ResetLock()
    {
        _isLocked = false;
        _teamLocked = null;
        _material.color = defaultColor;
        _rBody.isKinematic = false;
    }

    public void Lock(AgentScript.Team team)
    {
        if (_isLocked || _isGrabed) return;
        
        _isLocked = true;
        _teamLocked = team;

        _material.color = team == AgentScript.Team.Hider ? hiderColor : seekerColor;
        _rBody.isKinematic = true;
    }
    
    public void Unlock(AgentScript.Team team)
    {
        if (_isLocked && team == _teamLocked)
        {
            ResetLock();
        }
    }

    public bool Grab(Rigidbody agentRigidbody)
    {
        if (!_isLocked && !_isGrabed)
        {
            _rBody.mass = 0.02f;
            _isGrabed = true;
            return true;
        }

        return false;
    }

    public void Drop()
    {
        _isGrabed = false;
        _rBody.mass = 0.5f;
    }
    
    public void Respawn()
    {
        if (_config == null)
        {
            _config = _manager.GetConfig();
        }
        ResetLock();
        Drop();
        transform.localPosition = objectType == ObjectType.Box ?
            _manager.GetSpawnHelper().GetVectorInsideRoom() :
            _manager.GetSpawnHelper().GetVectorOutsideRoom();

        if (objectType == ObjectType.Box)
        {
            transform.rotation = Quaternion.identity;
            transform.Rotate(0, Random.value * 360 - 180f, 0);
        }
        else
        {
            transform.rotation = Quaternion.AngleAxis(-90, new Vector3(1, 0, 0));
            transform.Rotate(0, 0, Random.value * 360 - 180f);
        }

        Physics.SyncTransforms();
        int attempts = _config.maxRespawnAttempts;
        while (_manager.GetSpawnHelper().AnyCollisionDetected(transform) && attempts > 0)
        {
            transform.localPosition = objectType == ObjectType.Box ? 
                _manager.GetSpawnHelper().GetVectorInsideRoom() :
                _manager.GetSpawnHelper().GetVectorOutsideRoom();
            if (objectType == ObjectType.Box)
                transform.Rotate(0, Random.value * 360 - 180f, 0);
            else
                transform.Rotate(0, 0, Random.value * 360 - 180f);
            Physics.SyncTransforms();
            attempts--;
        }
        _rBody.angularVelocity = Vector3.zero;
        _rBody.velocity = Vector3.zero;
        Physics.SyncTransforms();
        //Debug.Log(attempts);
        //isSpawnNeeded = false;
    }
    // Start is called before the first frame update
    
    void Start()
    {
        _rBody = GetComponentInChildren<Rigidbody>();
        _material = GetComponentInChildren<MeshRenderer>().material;
        _manager = transform.parent.Find("ManagerObject").GetComponent<ObjectsManager>();
        _config = _manager.GetConfig(); 
        //Respawn();
    }

    // Update is called once per frame
    void Update()
    {
        // For debug
        if (Input.GetKey(KeyCode.U))
            Unlock(AgentScript.Team.Hider);
        if (Input.GetKey(KeyCode.I))
            Lock(AgentScript.Team.Hider);
        if (Input.GetKey(KeyCode.O))
            Unlock(AgentScript.Team.Seeker);
        if (Input.GetKey(KeyCode.P))
            Lock(AgentScript.Team.Seeker);
    }
}
