using Unity.Netcode;
using UnityEngine;
using TMPro; // Needed for UI text
using System.Collections;
using UnityEngine.SceneManagement;
public class GameManager : NetworkBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private string nextSceneName;
    [Header("Game Settings")]
    [SerializeField] private float roundDuration = 60f; // Let's give them a minute to finish

    [Header("UI Elements")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text winnerText; // A new text element to announce the winner
    [SerializeField] private GameObject votingPanel; // The panel with Yes/No buttons

    // A synced variable for the round timer
    private NetworkVariable<float> network_roundTimer = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // A synced variable to know if the round has ended (so we don't declare two winners)
    private NetworkVariable<bool> network_isRoundOver = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> yesVotes = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> noVotes = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> votingEnded = new NetworkVariable<bool>(false);
    public void ReceiveVote(bool vote)
    {
        // When a player votes, they tell the server via an RPC.
        SubmitVoteServerRpc(vote);
    }
    [ServerRpc(RequireOwnership = false)] // RequireOwnership=false is important here.
    private void SubmitVoteServerRpc(bool vote)
    {
        // If voting is already over, ignore this vote.
        if (votingEnded.Value) return;

        if (vote)
        {
            yesVotes.Value++;
        }
        else
        {
            noVotes.Value++;
        }

        // Check if all connected players have voted.
        int totalPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        if (yesVotes.Value + noVotes.Value >= totalPlayers)
        {
            TallyVotes();
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
    public override void OnNetworkSpawn()
    {
        // When the game starts, hide the winner text and voting panel
        winnerText.gameObject.SetActive(false);
        votingPanel.SetActive(false);

        // The server is in charge of starting the timer
        if (IsServer)
        {
            network_roundTimer.Value = roundDuration;
            network_isRoundOver.Value = false;
        }
    }

    private void Update()
    {
        // Update the timer text for everyone
        timerText.text = "Time: " + network_roundTimer.Value.ToString("F0");

        // --- SERVER-ONLY LOGIC ---
        if (!IsServer)
        {
            return;
        }

        // If the round is already over, do nothing.
        if (network_isRoundOver.Value)
        {
            return;
        }

        // Count down the timer
        network_roundTimer.Value -= Time.deltaTime;

        // Check if the timer has run out
        if (network_roundTimer.Value <= 0)
        {
            Debug.Log("SERVER: Timer ran out!");
            EndRound(false); // End the round, with no winner
        }
    }

    // This is a public function that the Player can call when they win
    public void PlayerReachedEndPoint(ulong winnerClientId)
    {
        // Double-check this only runs on the server and that the round isn't already over
        if (!IsServer || network_isRoundOver.Value)
        {
            return;
        }

        Debug.Log($"SERVER: Player {winnerClientId} has reached the end!");

        // Award a point to the winning player
        // We find the player's NetworkObject using their ClientId
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(winnerClientId, out NetworkClient client))
        {
            PlayerController winnerController = client.PlayerObject.GetComponent<PlayerController>();
            if (winnerController != null)
            {
                winnerController.PlayerScore.Value++;
            }
        }

        EndRound(true, winnerClientId); // End the round, with a winner
    }

    // This function handles the end-of-round logic
    private void EndRound(bool wasWon, ulong winnerClientId = 0)
    {
        network_isRoundOver.Value = true;

        // Announce the winner to everyone using a ClientRpc
        AnnounceWinnerClientRpc(wasWon, winnerClientId);

        // TODO: Start the voting process after a short delay
        StartCoroutine(StartVoteProcessAfterDelay(3.0f));
    }
    private IEnumerator StartVoteProcessAfterDelay(float delay)
    {
        // Wait for 'delay' seconds.
        yield return new WaitForSeconds(delay);

        // After waiting, call a ClientRpc to show the voting panel to everyone.
        ShowVotingPanelClientRpc();
    }
    [ClientRpc]
    private void ShowVotingPanelClientRpc()
    {
        // This code runs on every client's machine.
        votingPanel.SetActive(true);
    }
    [ClientRpc]
    private void AnnounceWinnerClientRpc(bool wasWon, ulong winnerClientId)
    {
        // This code runs on EVERYONE'S machine
        if (wasWon)
        {
            winnerText.text = $"Player {winnerClientId} Wins!";
        }
        else
        {
            winnerText.text = "Time's Up!";
        }
        winnerText.gameObject.SetActive(true);

        // TODO: Show the voting panel
        // votingPanel.SetActive(true);
    }
}