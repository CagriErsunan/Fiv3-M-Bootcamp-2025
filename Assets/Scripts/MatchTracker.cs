using UnityEngine;
using Unity.Netcode;

// This script will live on our persistent NetworkManager object.
public class MatchTracker : NetworkBehaviour
{
    // A public static instance so any GameManager can easily find it.
    public static MatchTracker Instance { get; private set; }

    [Header("Match Settings")]
    [SerializeField] private int roundsToWin = 5;
    [SerializeField] private string endSceneName = "GameScene2"; // This is our podium scene

    // Synced variable to track the current round number.
    public NetworkVariable<int> CurrentRound = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        // Set up the Singleton instance.
        // We use IsServer to ensure this only happens once.
        if (IsServer)
        {
            Instance = this;
        }
    }
    public bool IsGameOver()
    {
        return CurrentRound.Value >= roundsToWin;
    }
    // This is a public function that our GameManagers will call at the end of a round.
    public void Server_OnRoundComplete()
    {
        if (!IsServer) return;

        CurrentRound.Value++;
        Debug.Log($"SERVER: Round {CurrentRound.Value} complete.");

        // Have we reached the final round?
        if (IsGameOver())
        {
            NetworkManager.Singleton.SceneManager.LoadScene(endSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}