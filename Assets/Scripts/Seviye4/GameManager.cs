using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    private HashSet<ulong> alivePlayers = new HashSet<ulong>();
    private Dictionary<ulong, int> eliminationCallCount = new Dictionary<ulong, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[GameManager] Spawn points missing!");
            return Vector3.zero;
        }
        return spawnPoints[index].position;
    }

    public Quaternion GetSpawnRotation(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[GameManager] Spawn points missing!");
            return Quaternion.identity;
        }
        return spawnPoints[index].rotation;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[GameManager] Client connected: {clientId}");
        alivePlayers.Add(clientId);
        SpawnPlayer(clientId);
        PrintAlivePlayers();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (alivePlayers.Remove(clientId))
        {
            Debug.Log($"[GameManager] Client disconnected and removed: {clientId}");
            CheckForWinner();
        }
        PrintAlivePlayers();
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (playerPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[GameManager] playerPrefab or spawnPoints missing.");
            return;
        }

        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Vector3 spawnPos = GetSpawnPosition(spawnIndex);
        Quaternion spawnRot = GetSpawnRotation(spawnIndex);

        GameObject playerObj = Instantiate(playerPrefab, spawnPos, spawnRot);
        var netObj = playerObj.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId);

        Debug.Log($"[GameManager] Spawned player for clientId: {clientId} at spawnPoint #{spawnIndex}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyPlayerEliminatedServerRpc(ulong playerId)
    {
        Debug.Log($"[GameManager] NotifyPlayerEliminatedServerRpc called for playerId {playerId}");

        if (!alivePlayers.Contains(playerId))
        {
            Debug.LogWarning($"[GameManager] Elimination requested for playerId {playerId} but player is NOT alive.");
            return;
        }

        alivePlayers.Remove(playerId);

        if (!eliminationCallCount.ContainsKey(playerId))
            eliminationCallCount[playerId] = 0;
        eliminationCallCount[playerId]++;
        Debug.Log($"[GameManager] Player {playerId} eliminated. Elimination calls: {eliminationCallCount[playerId]}");

        ShowGameOverClientRpc(playerId);
        CheckForWinner();
        PrintAlivePlayers();
    }

    private void CheckForWinner()
    {
        if (alivePlayers.Count == 1)
        {
            ulong winnerId = 0;
            foreach (var id in alivePlayers)
                winnerId = id;

            Debug.Log($"[GameManager] Winner found: {winnerId}");
            ShowWinnerClientRpc(winnerId);
        }
        else if (alivePlayers.Count == 0)
        {
            Debug.Log("[GameManager] No players alive!");
        }
    }

    [ClientRpc]
    private void ShowWinnerClientRpc(ulong winnerId)
    {
        bool isWinner = (NetworkManager.Singleton.LocalClientId == winnerId);
        Debug.Log($"[GameManager] ShowWinnerClientRpc - IsLocalWinner: {isWinner}");

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver(isWinner);
        else
            Debug.LogWarning("[GameManager] UIManager instance missing on client.");
    }

    [ClientRpc]
    private void ShowGameOverClientRpc(ulong eliminatedPlayerId)
    {
        bool isEliminated = (NetworkManager.Singleton.LocalClientId == eliminatedPlayerId);
        Debug.Log($"[GameManager] ShowGameOverClientRpc - IsLocalEliminated: {isEliminated}");

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver(!isEliminated); // Eliminen olanlar için false göster
        else
            Debug.LogWarning("[GameManager] UIManager instance missing on client.");
    }

    private void PrintAlivePlayers()
    {
        string players = string.Join(", ", alivePlayers);
        Debug.Log($"[GameManager] Alive players: {players}");
    }
}
