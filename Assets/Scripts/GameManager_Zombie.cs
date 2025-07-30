using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement; // Add this for scene management

public class GameManager_Zombie : NetworkBehaviour
{
    private enum GameState { GracePeriod, RoundInProgress, RoundOver }

    [Header("Scene Management")]
    [SerializeField] private string nextSceneName; // The scene to load if vote is "Yes"

    [Header("Game Settings")]
    [SerializeField] private float graceDuration = 5f;
    [SerializeField] private float roundDuration = 90f;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text mainTimerText;
    [SerializeField] private TMP_Text announcementText;
    [SerializeField] private GameObject votingPanel;

    // Synced Game State Variables
    private NetworkVariable<GameState> gameState = new NetworkVariable<GameState>();
    private NetworkVariable<float> timer = new NetworkVariable<float>();
    private NetworkVariable<ulong> alphaInfectedId = new NetworkVariable<ulong>();

    // --- NEW VOTING VARIABLES (Copied from your other GameManager) ---
    private NetworkVariable<int> yesVotes = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> noVotes = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> votingEnded = new NetworkVariable<bool>(false);


    // --- OnNetworkSpawn, Update, StartRound, etc. remain the same ---
    // (No changes needed in the top part of the script)
    #region Existing Zombie Logic
    // Inside GameManager_Zombie.cs

    public override void OnNetworkSpawn()
    {
        // Subscribe to state changes to update UI
        gameState.OnValueChanged += OnGameStateChanged;

        // This part runs only on the SERVER.
        if (IsServer)
        {
            // --- THE FIX IS HERE ---
            // Before starting the round, loop through all connected players and reset their role.
            Debug.Log("SERVER: Resetting all player roles to Survivor.");
            foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
            {
                // Get the PlayerController from the player's NetworkObject.
                PlayerController pc = client.PlayerObject.GetComponent<PlayerController>();
                if (pc != null)
                {
                    // Set their role back to the default.
                    pc.Role.Value = PlayerRole.Survivor;
                }
            }

            // --- Now, continue with the normal round start ---
            timer.Value = graceDuration;
            gameState.Value = GameState.GracePeriod;

            // We also need to reset our own voting state for the new round.
            votingEnded.Value = false;
            yesVotes.Value = 0;
            noVotes.Value = 0;
        }
    }
    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        announcementText.gameObject.SetActive(true);
        if (newState == GameState.GracePeriod) announcementText.text = "Get Ready!";
        if (newState == GameState.RoundInProgress) announcementText.gameObject.SetActive(false);
        if (newState == GameState.RoundOver) votingPanel.SetActive(false);
    }
    private void Update()
    {
        mainTimerText.text = "Time: " + timer.Value.ToString("F0");
        if (!IsServer || gameState.Value == GameState.RoundOver) return;
        timer.Value -= Time.deltaTime;
        if (timer.Value <= 0)
        {
            if (gameState.Value == GameState.GracePeriod) StartRound();
            else if (gameState.Value == GameState.RoundInProgress) EndRound(false);
        }
        if (gameState.Value == GameState.RoundInProgress) CheckForInfectedWin();
    }
    private void StartRound()
    {
        gameState.Value = GameState.RoundInProgress;
        timer.Value = roundDuration;
        List<ulong> playerIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();
        int randomIndex = Random.Range(0, playerIds.Count);
        ulong chosenPlayerId = playerIds[randomIndex];
        alphaInfectedId.Value = chosenPlayerId;
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(chosenPlayerId, out NetworkClient client))
        {
            client.PlayerObject.GetComponent<PlayerController>().Role.Value = PlayerRole.AlphaInfected;
        }
    }
    private void CheckForInfectedWin()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (client.PlayerObject.GetComponent<PlayerController>().Role.Value == PlayerRole.Survivor) return;
        }
        EndRound(true);
    }
    private void EndRound(bool infectedWon)
    {
        gameState.Value = GameState.RoundOver;
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            PlayerController pc = client.PlayerObject.GetComponent<PlayerController>();
            if (infectedWon)
            {
                if (pc.Role.Value == PlayerRole.AlphaInfected) pc.PlayerScore.Value += 2;
                else if (pc.Role.Value == PlayerRole.Infected) pc.PlayerScore.Value += 1;
            }
            else
            {
                if (pc.Role.Value == PlayerRole.Survivor) pc.PlayerScore.Value += 1;
            }
        }
        AnnounceWinnerClientRpc(infectedWon);
        StartCoroutine(StartVoteProcessAfterDelay(11.1f));
    }
    [ClientRpc]
    private void AnnounceWinnerClientRpc(bool infectedWon)
    {
        announcementText.gameObject.SetActive(true);
        announcementText.text = infectedWon ? "The Infected Win!" : "The Survivors Win!";
    }
    private IEnumerator StartVoteProcessAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowVotingPanelClientRpc();
    }
    #endregion
    [ClientRpc]
    private void ShowVotingPanelClientRpc()
    {
        // This code will now run on EVERYONE'S machine (Host and all Clients).
        if (votingPanel != null)
        {
            votingPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Voting Panel reference is missing on a client!");
        }
    }

    // --- NEW VOTING FUNCTIONS (Copied from your other GameManager) ---

    // This is the public function that your VotingUI script will call.
    // Make sure your VotingUI script is on the VotingPanel in GameScene3!
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
        Debug.Log($"SERVER: Voting has ended. Yes: {yesVotes.Value}, No: {noVotes.Value}");

        if (yesVotes.Value >= noVotes.Value)
        {
            Debug.Log($"SERVER: Vote PASSED! Loading {nextSceneName}.");
            // Use the variable to load the next scene
            NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("SERVER: Vote FAILED! Reloading current scene for another round.");
            // Dynamically get the current scene name to reload it
            string currentSceneName = SceneManager.GetActiveScene().name;
            NetworkManager.Singleton.SceneManager.LoadScene(currentSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}