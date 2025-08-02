using UnityEngine;
using TMPro; // Needed for the text prompt

public class ControlsKiosk : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private GameObject controlsPanel;

    // This function shows or hides the "[E] to Interact" prompt.
    public void ShowPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show);
        }
    }

    // This function is called by the player to open the main controls panel.
    public void ShowControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);

            // We can also hide the small prompt when the big panel is open.
            ShowPrompt(false);
        }
    }
}