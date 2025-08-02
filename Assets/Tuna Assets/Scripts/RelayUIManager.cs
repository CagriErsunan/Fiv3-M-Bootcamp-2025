using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class RelayUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Button hostButton;
    public Button joinButton;
    public Button exitButton;

    public GameObject joinCodePanel;
    public TMP_Text hostJoinCodeText;
    public TMP_InputField joinCodeInput;
    public Button joinConfirmButton;

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinPanelOpened);
        joinConfirmButton.onClick.AddListener(OnJoinClicked);
        exitButton.onClick.AddListener(() => Application.Quit());

        joinCodePanel.SetActive(false);
    }

    private async void OnHostClicked()
    {
        string joinCode = await RelayManager.Instance.CreateRelay(4); // max 4 oyuncu
        if (!string.IsNullOrEmpty(joinCode))
        {
            hostJoinCodeText.text = $"Join Code: {joinCode}";
            joinCodePanel.SetActive(true); // Host join code’u görecek
        }
    }

    private void OnJoinPanelOpened()
    {
        joinCodePanel.SetActive(true); // Client join paneli açýlýr
        hostJoinCodeText.text = "Enter Join Code Below";
    }

    private async void OnJoinClicked()
    {
        string code = joinCodeInput.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(code)) return;

        bool success = await RelayManager.Instance.JoinRelay(code);
        if (!success)
        {
            Debug.LogError("Join failed! Wrong code or Relay not active.");
        }
        else
        {
            joinCodePanel.SetActive(false);
        }
    }
}
