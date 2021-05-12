using UnityEngine;

public class Config : MonoBehaviour
{
    // Agents movement parameters
    public float agentsForceMultiplier = 35.0f;
    public float agentsRotationMultiplier = 2.0f;
    // Agents reward parameters
    public float rewardScale = 0.1f;
    public float penaltyForLeaving = -5.0f;
    // Objects spawn parameters
    public float spawnInsideRoomWidth = 4f;
    public float spawnInsideRoomOffset = 0.75f;
    public float spawnOutsideRoomWidth = 8.0f;
    public float spawnOutsideRoomOffset = 4.0f;
    public int maxRespawnAttempts = 20;
    // Walls parameters
    public float roomHolesSize = 2.3f;
    public float intersectionDistance = 2.3f;
    // Interaction with movables
    public float maxDistToInteractWithBox = 2f;
    // Episode parameters
    public int episodeLength = 1000;
    public int preparingPhaseLength = 400;
}
