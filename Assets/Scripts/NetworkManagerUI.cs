using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
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
                joinCodeInput.text = joinCode;
            buttonsPanel.SetActive(false);
        }
    }

    private async void OnClientClicked()
    {
        string code = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("Join code is empty!");
            return;
        }

        bool success = await RelayManager.Instance.JoinRelay(code);
        if (success)
        {
            buttonsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Failed to join relay with this code!");
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
