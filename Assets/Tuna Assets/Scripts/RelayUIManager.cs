using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelayUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField joinCodeInput;
    public Button startHostButton;
    public Button joinClientButton;

    private void Start()
    {
        startHostButton.onClick.AddListener(OnHostClicked);
        joinClientButton.onClick.AddListener(OnJoinClicked);
    }

    private async void OnHostClicked()
    {
        Debug.Log("[RelayUI] Host button clicked");
        string joinCode = await RelayManager.Instance.CreateRelay();
        if (!string.IsNullOrEmpty(joinCode))
        {
            Debug.Log("[RelayUI] Join code created: " + joinCode);
            if (joinCodeInput != null)
                joinCodeInput.text = joinCode; // Konsoldan kopyalamak yerine UI'da göster
        }
        else
        {
            Debug.LogError("[RelayUI] Failed to create Relay!");
        }
    }

    private async void OnJoinClicked()
    {
        string code = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("[RelayUI] Join code input is empty!");
            return;
        }

        Debug.Log("[RelayUI] Trying to join with code: " + code);
        bool success = await RelayManager.Instance.JoinRelay(code);
        if (!success)
        {
            Debug.LogError("[RelayUI] Join failed! Check console for Relay logs.");
        }
        else
        {
            Debug.Log("[RelayUI] Successfully joined Relay!");
        }
    }
}
