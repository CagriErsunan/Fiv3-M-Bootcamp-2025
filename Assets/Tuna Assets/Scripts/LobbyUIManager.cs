using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    public Transform playerListPanel;      // ScrollView>Viewport>Content
    public GameObject playerEntryPrefab;   // TMP_Text prefabý
    public Button readyButton;
    public Button startGameButton;
    public TMP_Text playerCountText;

    private List<GameObject> currentEntries = new List<GameObject>();

    void Start()
    {
        readyButton.onClick.AddListener(OnReadyClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);

        if (!NetworkManager.Singleton.IsServer)
            startGameButton.gameObject.SetActive(false);

        InvokeRepeating(nameof(UpdatePlayerList), 0.3f, 0.5f);
    }

    void OnReadyClicked()
    {
        var localPlayer = FindLocalLobbyPlayer();
        if (localPlayer != null)
            localPlayer.SetReadyServerRpc(!localPlayer.isReady.Value);
    }

    void OnStartGameClicked()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        foreach (var player in FindAllLobbyPlayers())
            if (!player.isReady.Value)
                return;

        NetworkManager.Singleton.SceneManager.LoadScene("RaceScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    void UpdatePlayerList()
    {
        foreach (var entry in currentEntries)
            Destroy(entry);
        currentEntries.Clear();

        var players = FindAllLobbyPlayers();
        playerCountText.text = "Oyuncu Sayýsý: " + players.Count;

        foreach (var player in players)
        {
            var entryObj = Instantiate(playerEntryPrefab, playerListPanel);
            var text = entryObj.GetComponent<TMP_Text>();
            string readyText = player.isReady.Value ? " (Ready)" : " (Not Ready)";
            text.text = player.playerName.Value.ToString() + readyText;
            currentEntries.Add(entryObj);
        }
    }

    LobbyPlayer FindLocalLobbyPlayer()
    {
        foreach (var player in FindAllLobbyPlayers())
            if (player.IsOwner)
                return player;
        return null;
    }
    List<LobbyPlayer> FindAllLobbyPlayers()
    {
        var players = new List<LobbyPlayer>();
        foreach (var obj in FindObjectsOfType<LobbyPlayer>())
            players.Add(obj);
        return players;
    }
}
