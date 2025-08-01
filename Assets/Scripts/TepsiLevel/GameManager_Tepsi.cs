using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager_Tepsi : NetworkBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private string nextSceneName;

    [Header("Game Settings")]
    [SerializeField] private float roundDuration = 60f;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private GameObject votingPanel;

    private NetworkVariable<float> network_roundTimer = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> network_isRoundOver = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> yesVotes = new NetworkVariable<int>(0);
    private NetworkVariable<int> noVotes = new NetworkVariable<int>(0);
    private NetworkVariable<bool> votingEnded = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        winnerText.gameObject.SetActive(false);
        votingPanel.SetActive(false);

        if (IsServer)
        {
            network_roundTimer.Value = roundDuration;
            network_isRoundOver.Value = false;
        }
    }

    private void Update()
    {
        timerText.text = "Time: " + network_roundTimer.Value.ToString("F0");

        if (!IsServer || network_isRoundOver.Value)
            return;

        network_roundTimer.Value -= Time.deltaTime;

        if (network_roundTimer.Value <= 0)
        {
            Debug.Log("SERVER: Round ended!");
            EndRound();
        }
    }

    private void EndRound()
    {
        network_isRoundOver.Value = true;

        List<ulong> survivors = new List<ulong>();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject.GetComponent<PlayerController>();
            if (player != null && !player.isEliminated.Value)
            {
                player.PlayerScore.Value += 1;
                survivors.Add(client.ClientId);
            }
        }

        AnnounceWinnersClientRpc(survivors.ToArray());
        StartCoroutine(StartVoteProcessAfterDelay(3.0f));
    }

    private IEnumerator StartVoteProcessAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowVotingPanelClientRpc();
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

        if (vote)
            yesVotes.Value++;
        else
            noVotes.Value++;

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
    private void AnnounceWinnersClientRpc(ulong[] survivorIds)
    {
        if (survivorIds.Length == 0)
        {
            winnerText.text = "No one survived!";
        }
        else
        {
            string players = string.Join(", ", survivorIds);
            winnerText.text = $"Survivors: {players}";
        }

        winnerText.gameObject.SetActive(true);
    }
}
