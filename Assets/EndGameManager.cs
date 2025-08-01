using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// This class is just a simple data container. It can stay here.
public class PlayerResult
{
    public ulong clientId;
    public int score;
}

public class EndGameManager : NetworkBehaviour
{
    [Header("Podium Transforms")]
    [SerializeField] private Transform firstPlacePodium;
    [SerializeField] private Transform secondPlacePodium;
    [SerializeField] private Transform thirdPlacePodium;
    [SerializeField] private Transform otherPlayersArea;

    [Header("Results UI")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private Transform resultsContainer;
    [SerializeField] private GameObject resultEntryPrefab;
    [SerializeField] private Sprite firstPlaceSprite;
    [SerializeField] private Sprite secondPlaceSprite;
    [SerializeField] private Sprite thirdPlaceSprite;

    [Header("Effects")]
    [SerializeField] private GameObject confettiVFXPrefab;
    [SerializeField] private Transform confettiSpawnPoint;

    [Header("End Match UI")]
    [SerializeField] private GameObject endMatchButtonsPanel;


    // This is the main server-side function that starts everything.
    [System.Obsolete]
    public override void OnNetworkSpawn()
    {
        // Hide the final buttons at the start.
        if (endMatchButtonsPanel != null)
        {
            endMatchButtonsPanel.SetActive(false);
        }

        if (!IsServer) return;

        BuildPodiumAndStartSequence();
    }

    [System.Obsolete]
    private void BuildPodiumAndStartSequence()
    {
        // 1. Get and sort the results.
        List<PlayerResult> finalResults = new List<PlayerResult>();
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (client.PlayerObject != null)
            {
                PlayerController pc = client.PlayerObject.GetComponent<PlayerController>();
                finalResults.Add(new PlayerResult { clientId = client.ClientId, score = pc.PlayerScore.Value });
            }
        }
        List<PlayerResult> sortedResults = finalResults.OrderByDescending(p => p.score).ToList();

        // 2. Teleport all players to their podium spots.
        for (int i = 0; i < sortedResults.Count; i++)
        {
            PlayerResult result = sortedResults[i];
            PlayerController playerToMove = NetworkManager.Singleton.ConnectedClients[result.clientId].PlayerObject.GetComponent<PlayerController>();
            if (playerToMove == null) continue;

            switch (i)
            {
                case 0: playerToMove.TeleportPlayer(firstPlacePodium.position, firstPlacePodium.rotation); break;
                case 1: playerToMove.TeleportPlayer(secondPlacePodium.position, secondPlacePodium.rotation); break;
                case 2: playerToMove.TeleportPlayer(thirdPlacePodium.position, thirdPlacePodium.rotation); break;
                default: playerToMove.TeleportPlayer(otherPlayersArea.position, otherPlayersArea.rotation); break;
            }
        }

        // 3. AFTER everyone is in position, send ONE command to all clients to start the visual sequence.
        StartEndSequenceClientRpc();
    }

    [ClientRpc]
    [System.Obsolete]
    private void StartEndSequenceClientRpc()
    {
        // This code now runs on EVERY client's machine.
        // It's responsible for the entire timed visual sequence.
        StartCoroutine(EndOfMatchSequence());
    }

    [System.Obsolete]
    private IEnumerator EndOfMatchSequence()
    {
        // --- Phase 1: Build and Show Results UI (10 seconds) ---
        BuildResultsUI();
        if (resultsPanel != null) resultsPanel.SetActive(true);
        yield return new WaitForSeconds(10f);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        // --- Phase 2: Confetti Celebration (5 seconds) ---
        SpawnConfetti();
        yield return new WaitForSeconds(5f);

        // --- Phase 3: Show Final Buttons ---
        if (endMatchButtonsPanel != null)
        {
            endMatchButtonsPanel.SetActive(true);
        }
    }

    // This is a local function now, called by the coroutine.
    [System.Obsolete]
    private void BuildResultsUI()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        List<PlayerResult> finalResults = new List<PlayerResult>();
        foreach (var player in players)
        {
            finalResults.Add(new PlayerResult { clientId = player.OwnerClientId, score = player.PlayerScore.Value });
        }
        List<PlayerResult> sortedResults = finalResults.OrderByDescending(p => p.score).ToList();

        foreach (Transform child in resultsContainer) Destroy(child.gameObject);

        for (int i = 0; i < sortedResults.Count && i < 3; i++)
        {
            PlayerResult result = sortedResults[i];
            GameObject entryGO = Instantiate(resultEntryPrefab, resultsContainer);
            Image rankImage = entryGO.transform.Find("RankImage").GetComponent<Image>();
            TMP_Text nameText = entryGO.transform.Find("PlayerNameText").GetComponent<TMP_Text>();
            TMP_Text scoreText = entryGO.transform.Find("PlayerScoreText").GetComponent<TMP_Text>();

            nameText.text = $"Player {result.clientId}";
            scoreText.text = result.score.ToString();

            if (i == 0 && firstPlaceSprite != null) rankImage.sprite = firstPlaceSprite;
            else if (i == 1 && secondPlaceSprite != null) rankImage.sprite = secondPlaceSprite;
            else if (i == 2 && thirdPlaceSprite != null) rankImage.sprite = thirdPlaceSprite;
        }
    }

    // This is also a local function now.
    private void SpawnConfetti()
    {
        if (confettiVFXPrefab != null && confettiSpawnPoint != null)
        {
            Instantiate(confettiVFXPrefab, confettiSpawnPoint.position, Quaternion.identity);
        }
    }

    // --- Button Click Functions ---
    public void OnReturnToLobbyClicked()
    {
        // To be safe, we only let the host initiate the shutdown sequence.
        if (IsHost)
        {
            // First, tell all clients to disconnect and go back to the lobby.
            ReturnToLobbyClientRpc();
        }
        else // If a client clicks it, they just disconnect themselves.
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
            SceneManager.LoadScene("LobbyScene");
        }
    }

    [ClientRpc]
    private void ReturnToLobbyClientRpc()
    {
        // This command is sent from the host to all clients.
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene("LobbyScene");
    }

    public void OnExitGameClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}