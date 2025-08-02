using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton; // join tuþu
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        exitButton.onClick.AddListener(OnExitGameClicked);
    }

    private async void OnHostClicked()
    {
        string joinCode = await RelayManager.Instance.CreateRelay();
        if (!string.IsNullOrEmpty(joinCode))
        {
            if (joinCodeInput != null)
                joinCodeInput.text = joinCode; // JoinCode UI'da göster
            buttonsPanel.SetActive(false);
        }
    }

    private async void OnClientClicked()
    {
        if (joinCodeInput == null)
        {
            Debug.LogError("JoinCodeInput is not assigned in Inspector!");
            return;
        }

        string code = joinCodeInput.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("Join code input is empty!");
            return;
        }

        clientButton.interactable = false; // spam engelle
        bool success = await RelayManager.Instance.JoinRelay(code);
        clientButton.interactable = true;

        if (success)
        {
            buttonsPanel.SetActive(false);
            Debug.Log("[RelayUI] Successfully joined Relay!");
        }
        else
        {
            Debug.LogError("[RelayUI] Join failed! Check console for Relay logs.");
        }
    }

    private void OnExitGameClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
