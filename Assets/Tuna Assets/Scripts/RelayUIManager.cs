using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelayUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField joinCodeInput;   // Client girecek
    [SerializeField] private TMP_Text joinCodeText;          // Host gösterecek
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button joinClientButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        startHostButton.onClick.AddListener(OnHostClicked);
        joinClientButton.onClick.AddListener(OnJoinClicked);
        exitButton.onClick.AddListener(OnExitGameClicked);

        joinCodeText.text = "";
        joinCodeInput.text = "";
    }

    // Host: Relay oluştur ve join code'u göster
    private async void OnHostClicked()
    {
        Debug.Log("[RelayUI] Host button clicked!");
        string joinCode = await RelayManager.Instance.CreateRelay();

        if (!string.IsNullOrEmpty(joinCode))
        {
            joinCodeText.text = "Join Code: " + joinCode;
            Debug.Log("[RelayUI] Relay created. Join Code: " + joinCode);
        }
        else
        {
            Debug.LogError("[RelayUI] Failed to create Relay!");
        }
    }

    // Client: Join code ile bağlan
    private async void OnJoinClicked()
    {
        string code = joinCodeInput.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("[RelayUI] Join code is empty!");
            return;
        }

        Debug.Log("[RelayUI] Trying to join Relay with code: " + code);
        bool success = await RelayManager.Instance.JoinRelay(code);

        if (success)
            Debug.Log("[RelayUI] Successfully joined Relay!");
        else
            Debug.LogError("[RelayUI] Join failed!");
    }

    private void OnExitGameClicked()
    {
        Debug.Log("[RelayUI] Exit button clicked. Quitting...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
