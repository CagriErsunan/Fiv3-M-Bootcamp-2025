using UnityEngine;
using System.Collections.Generic; // Needed to use Lists

public class PredatorZoneController : MonoBehaviour
{
    // A list to keep track of all survivor players currently in our trigger zone.
    public List<PlayerController> SurvivorsInZone = new List<PlayerController>();

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is a player.
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            // Check if that player is a survivor and not already in our list.
            if (player.Role.Value == PlayerRole.Survivor && !SurvivorsInZone.Contains(player))
            {
                // If so, add them to our list.
                SurvivorsInZone.Add(player);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object that left is a player.
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            // If they are in our list, remove them.
            if (SurvivorsInZone.Contains(player))
            {
                SurvivorsInZone.Remove(player);
            }
        }
    }
}