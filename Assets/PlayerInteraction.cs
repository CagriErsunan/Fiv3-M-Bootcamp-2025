using UnityEngine;
using Unity.Netcode; // Needed for IsOwner

public class PlayerInteraction : NetworkBehaviour
{
    // A variable to keep track of the kiosk we are currently near.
    private ControlsKiosk currentKiosk;

    // This runs on the local player's machine.
    private void Update()
    {
        // We only want the owner to be able to interact.
        if (!IsOwner) return;

        // If we are near a kiosk and we press the 'E' key...
        if (currentKiosk != null && Input.GetKeyDown(KeyCode.E))
        {
            // ...tell that kiosk to show its controls.
            currentKiosk.ShowControls();
        }
    }

    // When we enter a trigger...
    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        // ...check if it's a ControlsKiosk.
        if (other.TryGetComponent<ControlsKiosk>(out ControlsKiosk kiosk))
        {
            // If it is, store it and show the prompt.
            currentKiosk = kiosk;
            currentKiosk.ShowPrompt(true);
        }
    }

    // When we leave a trigger...
    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        // ...check if it's the same kiosk we were just in.
        if (other.TryGetComponent<ControlsKiosk>(out ControlsKiosk kiosk) && kiosk == currentKiosk)
        {
            // If it is, hide the prompt and clear our reference.
            currentKiosk.ShowPrompt(false);
            currentKiosk = null;
        }
    }
}