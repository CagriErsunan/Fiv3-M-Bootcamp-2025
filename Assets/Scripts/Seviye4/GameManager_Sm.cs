using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager_Sm : NetworkBehaviour
{
    public static GameManager_Sm Instance;

    [Header("Scene Management")]
    [SerializeField] private string nextSceneName;

    [Header("Game Settings")]
    [SerializeField] private float roundDuration = 60f;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private GameObject votingPanel;

    [Header("Spawn Settings")]
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    private HashSet<ulong> alivePlayers = new HashSet<ulong>();
    private Dictionary<ulong, int> eliminationCallCount = new Dictionary<ulong, int>();

    private NetworkVariable<float> network_roundTimer = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> network_isRoundOver = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> yesVotes = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> noVotes = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> votingEnded = new NetworkVariable<bool>(false);

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

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            network_roundTimer.Value = roundDuration;
            network_isRoundOver.Value = false;
        }

        winnerText.gameObject.SetActive(false);
        votingPanel.SetActive(false);
    }

    private void Update()
    {
        timerText.text = "Time: " + network_roundTimer.Value.ToString("F0");

        if (!IsServer || network_isRoundOver.Value) return;

        network_roundTimer.Value -= Time.deltaTime;
        if (network_roundTimer.Value <= 0)
        {
            Debug.Log("SERVER: Timer ran out!");
            EndRound(false);
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
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Vector3 spawnPos = spawnPoints[spawnIndex].position;
        Quaternion spawnRot = spawnPoints[spawnIndex].rotation;

        GameObject playerObj = Instantiate(playerPrefab, spawnPos, spawnRot);
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyPlayerEliminatedServerRpc(ulong playerId)
    {
        if (!alivePlayers.Contains(playerId)) return;

        alivePlayers.Remove(playerId);

        if (!eliminationCallCount.ContainsKey(playerId))
            eliminationCallCount[playerId] = 0;
        eliminationCallCount[playerId]++;

        ShowGameOverClientRpc(playerId);
        CheckForWinner();
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
            EndRound(true, winnerId);
        }
        else if (alivePlayers.Count == 0)
        {
            Debug.Log("[GameManager] No players alive!");
            EndRound(false);
        }
    }

    public void PlayerReachedEndPoint(ulong winnerClientId)
    {
        if (!IsServer || network_isRoundOver.Value) return;

        Debug.Log($"SERVER: Player {winnerClientId} reached end!");

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(winnerClientId, out var client))
        {
            var pc = client.PlayerObject.GetComponent<PlayerController>();
            if (pc != null)
                pc.PlayerScore.Value++;
        }

        EndRound(true, winnerClientId);
    }

    private void EndRound(bool wasWon, ulong winnerClientId = 0)
    {
        network_isRoundOver.Value = true;
        AnnounceWinnerClientRpc(wasWon, winnerClientId);
        StartCoroutine(StartVoteProcessAfterDelay(3f));
    }

    private IEnumerator StartVoteProcessAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowVotingPanelClientRpc();
    }

    [ClientRpc]
    private void AnnounceWinnerClientRpc(bool wasWon, ulong winnerClientId)
    {
        winnerText.text = wasWon ? $"Player {winnerClientId} Wins!" : "Time's Up!";
        winnerText.gameObject.SetActive(true);
    }

    [ClientRpc]
    private void ShowVotingPanelClientRpc()
    {
        votingPanel.SetActive(true);
    }

    public void ReceiveVote(bool vote)
    {
        SubmitVoteServerRpc(vote);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitVoteServerRpc(bool vote)
    {
        if (votingEnded.Value) return;

        if (vote) yesVotes.Value++;
        else noVotes.Value++;

        int totalPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        if (yesVotes.Value + noVotes.Value >= totalPlayers)
        {
            TallyVotes();
        }
    }

    private void TallyVotes()
    {
        votingEnded.Value = true;
        if (yesVotes.Value >= noVotes.Value)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
        else
        {
            string currentScene = SceneManager.GetActiveScene().name;
            NetworkManager.Singleton.SceneManager.LoadScene(currentScene, LoadSceneMode.Single);
        }
    }

    [ClientRpc]
    private void ShowWinnerClientRpc(ulong winnerId)
    {
        bool isWinner = NetworkManager.Singleton.LocalClientId == winnerId;
        UIManager_Sm.Instance?.ShowGameOver(isWinner);
    }

    [ClientRpc]
    private void ShowGameOverClientRpc(ulong eliminatedPlayerId)
    {
        bool isEliminated = NetworkManager.Singleton.LocalClientId == eliminatedPlayerId;
        UIManager_Sm.Instance?.ShowGameOver(!isEliminated);
    }

    private void PrintAlivePlayers()
    {
        Debug.Log("Alive players: " + string.Join(", ", alivePlayers));
    }
}
