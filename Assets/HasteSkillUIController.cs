using UnityEngine;
using UnityEngine.UI; // Needed for Image
using TMPro; // In case you want to add a text timer

public class HasteSkillUIController : MonoBehaviour
{
    [SerializeField] private Image cooldownWipeImage;
    [SerializeField] private GameObject activeFrame;
    // This function will be called by the PlayerController to update the UI
    public void UpdateCooldown(float currentTime, float maxTime)
    {
        // If the max time is 0, it means we are not on cooldown.
        if (maxTime <= 0)
        {
            cooldownWipeImage.fillAmount = 0;
            return;
        }

        // Calculate the fill amount (a value from 0 to 1)
        cooldownWipeImage.fillAmount = 1f - (currentTime / maxTime);
    }
    public void SetActiveState(bool isActive)
    {
        if (activeFrame != null)
        {
            activeFrame.SetActive(isActive);
        }
    }
}