using Unity.Netcode;
using UnityEngine;
using TMPro;

// This MUST be a NetworkBehaviour to use IsServer and NetworkVariables
public class LobbyManager : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private int minPlayersToStart = 2;
    [SerializeField] private float countdownDuration = 10f;

    [Header("UI References")]
    [SerializeField] private TMP_Text lobbyStatusText;

    // A synced variable to hold our timer. Only the server can change it.
    private NetworkVariable<float> network_countdownTimer = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // A flag to ensure the countdown only starts once.
    private NetworkVariable<bool> network_isCountdownStarted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        // When this object spawns, if we are the server, initialize the text.
        if (IsServer)
        {
            lobbyStatusText.text = "Waiting for players...";
        }
    }

    private void Update()
    {
        // UI update logic now runs on EVERYONE and reads from the synced variables.

        // Check the value of our NEW synced variable.
        if (network_isCountdownStarted.Value)
        {
            // If the countdown has begun, display the time.
            lobbyStatusText.text = $"Game starting in: {network_countdownTimer.Value:F0}";
        }
        else
        {
            // If waiting, show this message.
            lobbyStatusText.text = $"Waiting for more players ... ({NetworkManager.Singleton.ConnectedClients.Count}/{minPlayersToStart})";
        }


        // --- SERVER-ONLY LOGIC ---
        if (!IsServer)
        {
            return;
        }

        // Check if we should start the countdown.
        // We use our new synced variable here as well.
        if (!network_isCountdownStarted.Value && NetworkManager.Singleton.ConnectedClients.Count >= minPlayersToStart)
        {
            // Set the synced variable to true. This change is sent to all clients.
            network_isCountdownStarted.Value = true;
            network_countdownTimer.Value = countdownDuration;
            Debug.Log("Minimum players reached! Starting countdown.");
        }

        // If the countdown is running, tick it down.
        if (network_isCountdownStarted.Value && network_countdownTimer.Value > 0)
        {
            network_countdownTimer.Value -= Time.deltaTime;

            if (network_countdownTimer.Value <= 0)
            {
                network_countdownTimer.Value = 0;
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene1", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
    }

}