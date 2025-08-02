using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager_BossArena : NetworkBehaviour
{
    // A state machine to control the flow of the round.
    private enum GameState { RoundInProgress, RoundOver }
    private NetworkVariable<GameState> network_gameState = new NetworkVariable<GameState>(GameState.RoundInProgress);

    [Header("Difficulty Scaling")]
    [SerializeField] private float timePerTier = 15f;
    [Header("AI Learning (Heatmap)")]
    [Tooltip("The size of the arena to track. Should match your boss's arenaSize.")]
    [SerializeField] private Vector2 heatmapAreaSize = new Vector2(20, 20);
    [Tooltip("How many cells the grid has. 10x10 is a good start.")]
    [SerializeField] private int heatmapResolution = 10;
    [Header("Scene Management")]
    [SerializeField] private string nextSceneName;

    [Header("UI")]
    [SerializeField] private GameObject votingPanel;
    [SerializeField] private TMPro.TMP_Text winnerText;

    // Server-side variables
    private float heatmapSampleTimer = 0f;
    private float heatmapSampleRate = 0.25f; // Sample 4 times per second
    private float tierTimer;
    private BossAIController boss;
    private float[,] heatmap;
    // Networked variables for voting
    private NetworkVariable<int> network_difficultyTier = new NetworkVariable<int>(0);
    private NetworkVariable<int> yesVotes = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> noVotes = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> votingEnded = new NetworkVariable<bool>(false);

    [System.Obsolete]
    public override void OnNetworkSpawn()
    {
        // Server sets up the initial state of the round.
        if (IsServer)
        {
            boss = FindObjectOfType<BossAIController>();
            tierTimer = timePerTier;

            // Reset all state variables for a clean round.
            network_gameState.Value = GameState.RoundInProgress;
            network_difficultyTier.Value = 0;
            UpdateBossDifficulty(); // Set initial difficulty
            InitializeHeatmap();
            votingEnded.Value = false;
            yesVotes.Value = 0;
            noVotes.Value = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
            {
                // Get their PlayerController.
                PlayerController pc = client.PlayerObject.GetComponent<PlayerController>();
                if (pc != null)
                {
                    // Authoritatively reset their health and elimination status for the new round.
                    pc.Health.Value = pc.maxHealth; // Using the public maxHealth from the controller
                    pc.IsEliminated.Value = false;
                }
            }
        }

        // Everyone hides the end-of-round UI when they spawn in.
        if (winnerText != null) winnerText.gameObject.SetActive(false);
        if (votingPanel != null) votingPanel.SetActive(false);
    }
    private void InitializeHeatmap()
    {
        Debug.Log("Server: Initializing AI learning heatmap.");
        // Create a new 2D array with the specified resolution.
        heatmap = new float[heatmapResolution, heatmapResolution];
    }
    private void Update()
    {
        // The server's main loop. If not server or round is over, do nothing.
        if (!IsServer || network_gameState.Value == GameState.RoundOver)
        {
            return;
        }

        // Handle difficulty scaling over time.
        tierTimer -= Time.deltaTime;
        if (tierTimer <= 0)
        {
            tierTimer = timePerTier;
            network_difficultyTier.Value++;
            UpdateBossDifficulty();
        }

        // Handle checking for the win condition.
        CheckForWinCondition();
        // --- NEW: AI LEARNING LOGIC ---
        heatmapSampleTimer -= Time.deltaTime;
        if (heatmapSampleTimer <= 0f)
        {
            heatmapSampleTimer = heatmapSampleRate;
            UpdateHeatmap();
        }
    }
    private void UpdateHeatmap()
    {
        // Loop through all connected players.
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            // Make sure the player is valid and is not eliminated.
            if (client.PlayerObject != null && !client.PlayerObject.GetComponent<PlayerController>().IsEliminated.Value)
            {
                // Get the player's position.
                Vector3 playerPosition = client.PlayerObject.transform.position;

                // --- Convert world position to grid coordinates ---
                // Calculate how big each cell is.
                float cellWidth = heatmapAreaSize.x / heatmapResolution;
                float cellHeight = heatmapAreaSize.y / heatmapResolution;

                // Convert the player's X/Z world position into an X/Y grid index.
                // We add (heatmapAreaSize / 2) to shift the origin from the center to the corner.
                int gridX = Mathf.FloorToInt((playerPosition.x + heatmapAreaSize.x / 2) / cellWidth);
                int gridY = Mathf.FloorToInt((playerPosition.z + heatmapAreaSize.y / 2) / cellHeight);

                // --- Add "heat" to the corresponding grid cell ---
                // First, check if the calculated coordinates are valid (within the grid).
                if (gridX >= 0 && gridX < heatmapResolution && gridY >= 0 && gridY < heatmapResolution)
                {
                    // If they are valid, increase the heat value for that cell.
                    heatmap[gridX, gridY] += 1.0f; // Add 1 "heat point"
                }
            }
        }
    }

    private void CheckForWinCondition()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count == 0) return;

        var livingPlayers = NetworkManager.Singleton.ConnectedClients.Values
            .Where(c => c.PlayerObject != null && !c.PlayerObject.GetComponent<PlayerController>().IsEliminated.Value)
            .ToList();

        int livingPlayerCount = livingPlayers.Count;

        // Check if only one (or zero) player is left.
        if (NetworkManager.Singleton.ConnectedClients.Count > 1 && livingPlayerCount <= 1)
        {
            EndRound(livingPlayers);
        }
        else if (NetworkManager.Singleton.ConnectedClients.Count == 1 && livingPlayerCount == 0)
        {
            EndRound(livingPlayers);
        }
    }
    public Vector3 GetHottestTargetPosition()
    {
        float maxHeat = 0f;
        int bestX = 0;
        int bestY = 0;

        // Loop through every single cell in our heatmap grid.
        for (int x = 0; x < heatmapResolution; x++)
        {
            for (int y = 0; y < heatmapResolution; y++)
            {
                // Is this cell's heat value greater than the hottest we've found so far?
                if (heatmap[x, y] > maxHeat)
                {
                    // If yes, this is our new "hottest" spot.
                    maxHeat = heatmap[x, y];
                    bestX = x;
                    bestY = y;
                }
            }
        }

        // --- "Cool down" the chosen spot ---
        // To prevent the boss from always attacking the exact same spot,
        // we'll reduce the heat of the chosen cell after we use it.
        if (maxHeat > 0)
        {
            heatmap[bestX, bestY] /= 2; // Cut the heat in half
        }


        // --- Convert the winning grid coordinate back to a world position ---
        float cellWidth = heatmapAreaSize.x / heatmapResolution;
        float cellHeight = heatmapAreaSize.y / heatmapResolution;

        // This is the reverse of the calculation we did in UpdateHeatmap.
        float worldX = (bestX * cellWidth) - (heatmapAreaSize.x / 2) + (cellWidth / 2);
        float worldZ = (bestY * cellHeight) - (heatmapAreaSize.y / 2) + (cellHeight / 2);

        // Return the final world position for the attack.
        return new Vector3(worldX, 0, worldZ);
    }
    private void UpdateBossDifficulty()
    {
        if (boss != null)
        {
            boss.SetDifficulty(network_difficultyTier.Value);
        }
    }

    private void EndRound(List<NetworkClient> winners)
    {
        // Set the state to RoundOver to stop the Update loop.
        network_gameState.Value = GameState.RoundOver;
        Debug.Log("Round is over!");

        if (boss != null)
        {
            boss.StopAI();
        }

        ulong winnerId = 0;
        if (winners.Count == 1)
        {
            PlayerController winnerController = winners[0].PlayerObject.GetComponent<PlayerController>();
            if (winnerController != null)
            {
                winnerController.PlayerScore.Value++;
                winnerId = winnerController.OwnerClientId;
            }
        }

        // Tell all clients to show the winner and start the vote process.
        AnnounceWinnerAndStartVoteClientRpc(winners.Count == 1, winnerId);
    }

    [ClientRpc]
    private void AnnounceWinnerAndStartVoteClientRpc(bool hasWinner, ulong winnerId)
    {
        if (winnerText != null)
        {
            winnerText.gameObject.SetActive(true);
            winnerText.text = hasWinner ? $"Player {winnerId} Wins!" : "Draw!";
        }
        StartCoroutine(ShowVotePanelAfterDelay(3.0f));
    }

    private IEnumerator ShowVotePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (winnerText != null) winnerText.gameObject.SetActive(false);
        if (votingPanel != null) votingPanel.SetActive(true);
    }

    // --- Voting Functions ---
    public void ReceiveVote(bool vote)
    {
        Debug.Log("CLIENT: Vote button clicked. Sending vote to server.");
        SubmitVoteServerRpc(vote);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitVoteServerRpc(bool vote, ServerRpcParams rpcParams = default)
    {
        if (votingEnded.Value) return;

        if (vote) yesVotes.Value++;
        else noVotes.Value++;

        int totalPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        Debug.Log($"SERVER: Received a vote. Current votes: Yes={yesVotes.Value}, No={noVotes.Value}. Total Players: {totalPlayers}");

        if (yesVotes.Value + noVotes.Value >= totalPlayers)
        {
            Debug.Log("SERVER: All votes are in! Calling TallyVotes...");
            TallyVotes();
        }
        else
        {
            Debug.Log("SERVER: Waiting for more votes...");
        }
    }

    private void TallyVotes()
    {
        votingEnded.Value = true;

        // --- THE NEW LOGIC ---

        // 1. Increment the round counter.
        GameData.CurrentRound++;
        Debug.Log($"SERVER: Round {GameData.CurrentRound} has ended.");

        // 2. Check if the game is over.
        if (GameData.CurrentRound >= GameData.RoundsToWin)
        {
            // --- GAME OVER ---
            Debug.Log("SERVER: Final round reached! Loading End Scene.");
            // Directly load our End Scene (GameScene2).
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene2", LoadSceneMode.Single);
        }
        else
        {
            // --- GAME CONTINUES ---
            // If the game is not over, run the normal voting logic.
            if (yesVotes.Value >= noVotes.Value)
            {
                Debug.Log($"SERVER: Vote PASSED! Loading {nextSceneName}.");
                NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.Log("SERVER: Vote FAILED! Reloading current scene.");
                string currentSceneName = SceneManager.GetActiveScene().name;
                NetworkManager.Singleton.SceneManager.LoadScene(currentSceneName, LoadSceneMode.Single);
            }
        }
    }
    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        // Wait for just one frame.
        // This gives the NetworkManager a moment to finish processing any other messages
        // before we hit it with a big scene load command.
        yield return null;

        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}