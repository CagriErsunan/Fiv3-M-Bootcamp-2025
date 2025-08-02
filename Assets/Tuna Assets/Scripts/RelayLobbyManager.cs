using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TMPro;

public class RelayLobbyManager : NetworkBehaviour
{
    [Header("Lobby Settings")]
    [SerializeField] private int minPlayersToStart = 2;
    [SerializeField] private float countdownDuration = 10f;
    [SerializeField] private string gameSceneName = "GameScene1";

    [Header("UI")]
    [SerializeField] private TMP_Text lobbyStatusText;

    private NetworkVariable<float> countdown = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> countdownStarted = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Update()
    {
        // 🔹 Herkes kendi UI'sini günceller
        if (!countdownStarted.Value)
        {
            lobbyStatusText.text = $"Waiting for players... ({NetworkManager.Singleton.ConnectedClients.Count}/{minPlayersToStart})";
        }
        else
        {
            lobbyStatusText.text = $"Starting in {countdown.Value:F0}...";
        }

        // 🔹 Sadece server countdown'u yönetir
        if (!IsServer) return;

        // Minimum oyuncu geldiyse countdown başlar
        if (!countdownStarted.Value && NetworkManager.Singleton.ConnectedClients.Count >= minPlayersToStart)
        {
            countdownStarted.Value = true;
            countdown.Value = countdownDuration;
        }

        // Countdown çalışıyorsa azalt
        if (countdownStarted.Value && countdown.Value > 0)
        {
            countdown.Value -= Time.deltaTime;
            if (countdown.Value <= 0)
            {
                countdown.Value = 0;
                StartGame();
            }
        }
    }

    private void StartGame()
    {
        if (!IsServer) return;

        Debug.Log("[Lobby] Countdown finished. Loading game scene...");
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
