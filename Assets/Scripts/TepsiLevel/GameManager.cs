using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections; // Add this for FixedString

public class GameManagerTepsi : NetworkBehaviour
{
    public static GameManagerTepsi InstanceUI { get; private set; }
    public NetworkVariable<float> roundTimer = new NetworkVariable<float>(70f);

    // Use FixedString128Bytes instead of string
    public NetworkVariable<FixedString128Bytes> roundEndMessage =
        new NetworkVariable<FixedString128Bytes>();

    public NetworkVariable<FixedString512Bytes> scoreText =
        new NetworkVariable<FixedString512Bytes>();

    private List<ulong> activePlayerClientIds = new List<ulong>();
    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    private bool isRoundOver = false;

    void Awake()
    {
        if (InstanceUI != null && InstanceUI != this)
        {
            Destroy(gameObject);
        }
        else
        {
            InstanceUI = this;
        }
    }

    void Update()
    {
        if (!IsServer || isRoundOver) return;

        if (roundTimer.Value > 0)
        {
            roundTimer.Value -= Time.deltaTime;
        }
        else
        {
            EndRound("Süre doldu!");
        }
    }

    public void PlayerDied(ulong deadPlayerClientId)
    {
        if (!IsServer || isRoundOver) return;

        if (activePlayerClientIds.Contains(deadPlayerClientId))
        {
            activePlayerClientIds.Remove(deadPlayerClientId);
        }

        if (activePlayerClientIds.Count == 1)
        {
            ulong winnerId = activePlayerClientIds[0];
            EndRound($"Oyuncu {winnerId} kazandı!");
        }
        else if (activePlayerClientIds.Count == 0)
        {
            EndRound("Herkes elendi!");
        }
    }

    private void EndRound(string reason)
    {
        if (isRoundOver) return;
        isRoundOver = true;

        // Update scores
        if (activePlayerClientIds.Count > 1)
        {
            foreach (var id in activePlayerClientIds)
            {
                playerScores[id] += 1;
            }
        }

        // Build score text
        string scoreString = "";
        foreach (var scoreEntry in playerScores)
        {
            scoreString += $"Oyuncu {scoreEntry.Key}: {scoreEntry.Value} Puan\n";
        }

        // Use FixedString for network transmission
        roundEndMessage.Value = reason;
        scoreText.Value = scoreString;

        AnnounceWinnerClientRpc();
    }

    [ClientRpc]
    private void AnnounceWinnerClientRpc()
    {
        Debug.Log($"TUR SONU: {roundEndMessage.Value}");

        if (UIManagerTepsi.InstanceUI != null)
        {
            UIManagerTepsi.InstanceUI.ShowScoreScreen(
                roundEndMessage.Value.ToString(),
                scoreText.Value.ToString()
            );
        }
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            activePlayerClientIds.Add(client.ClientId);
            playerScores[client.ClientId] = 0;
        }

        // Yeni oyuncular eklendikçe listeye almak için:
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (!activePlayerClientIds.Contains(clientId))
        {
            activePlayerClientIds.Add(clientId);
            playerScores[clientId] = 0;
        }
    }
}