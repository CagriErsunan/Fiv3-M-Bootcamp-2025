using UnityEngine;

public class DetectionZoneController : MonoBehaviour
{
    private EnemyAIController aiController;

    void Start()
    {
        // Get a reference to the main AI script on our parent object.
        aiController = GetComponentInParent<EnemyAIController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // If a player enters our trigger, tell the main AI.
        if (other.CompareTag("Player"))
        {
            aiController.OnPlayerEnterDetectionZone(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If a player leaves, tell the main AI.
        if (other.CompareTag("Player"))
        {
            aiController.OnPlayerExitDetectionZone(other.transform);
        }
    }
}