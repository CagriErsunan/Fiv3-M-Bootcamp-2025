using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using static NetworkManagerKurulum;

public static class LobbyData
{
    public static string PlayerName = "";
    public static string LobbyName = "";
}

// MainMenu sahnesinde bir boþ GameObject'e ekle, Inspector'dan alanlarý ata.
public class MainMenuUI : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public TMP_InputField lobbyNameInput;
    public GameObject warningPanel;
    public TMP_Text warningText;

    public void OnCreateLobbyClicked()
    {
        LobbyData.PlayerName = playerNameInput.text.Trim();
        LobbyData.LobbyName = lobbyNameInput.text.Trim();

        if (LobbyData.LobbyName == "" || LobbyData.PlayerName == "")
        {
            warningPanel.SetActive(true);
            warningText.text = "Lobi adý ve oyuncu adý boþ olamaz!";
            return;
        }

        LobbyServer.Instance.SetLobbyName(LobbyData.LobbyName);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(LobbyData.LobbyName);
        NetworkManager.Singleton.StartHost();
        SceneManager.LoadScene("LobbyScene");
    }

    public void OnJoinLobbyClicked()
    {
        LobbyData.PlayerName = playerNameInput.text.Trim();
        LobbyData.LobbyName = lobbyNameInput.text.Trim();

        if (LobbyData.LobbyName == "" || LobbyData.PlayerName == "")
        {
            warningPanel.SetActive(true);
            warningText.text = "Lobi adý ve oyuncu adý boþ olamaz!";
            return;
        }

        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(LobbyData.LobbyName);
        NetworkManager.Singleton.StartClient();
        SceneManager.LoadScene("LobbyScene");
    }
}
