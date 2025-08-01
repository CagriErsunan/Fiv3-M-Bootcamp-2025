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

    [Header("Scene Management")]
    [SerializeField] private string nextSceneName;

    [Header("UI")]
    [SerializeField] private GameObject votingPanel;
    [SerializeField] private TMPro.TMP_Text winnerText;

    // Server-side variables
    private float tierTimer;
    private BossAIController boss;

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