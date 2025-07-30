using Unity.Netcode;
using UnityEngine;
using TMPro; // Needed for TextMeshPro

public class ScoreboardManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject scoreboardPanel;
    [SerializeField] private Transform entryContainer; // The parent for our score entries
    [SerializeField] private GameObject scoreEntryPrefab; // The template for a single score entry

    private void Start()
    {
        // Make sure the scoreboard is hidden at the start.
        scoreboardPanel.SetActive(false);
    }

    private void Update()
    {
        // Show the scoreboard when TAB is held down
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("TAB KEY PRESSED!");
            scoreboardPanel.SetActive(true);
            RefreshScoreboard();
        }

        // Hide the scoreboard when TAB is released
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            Debug.Log("TAB KEY RELEASED!");
            scoreboardPanel.SetActive(false);
        }
    }

    private void RefreshScoreboard()
    {
        // --- Step 1: Clear out any old score entries ---
        foreach (Transform child in entryContainer)
        {
            Destroy(child.gameObject);
        }

        // --- Step 2: Create a new entry for each connected player ---
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            // Instantiate the prefab from our project files
            GameObject newEntry = Instantiate(scoreEntryPrefab, entryContainer);

            // Get the text components from the new entry
            TMP_Text playerNameText = newEntry.transform.Find("PlayerNameText").GetComponent<TMP_Text>();
            TMP_Text playerScoreText = newEntry.transform.Find("PlayerScoreText").GetComponent<TMP_Text>();

            // Get the player's controller script to read their score
            PlayerController playerController = client.PlayerObject.GetComponent<PlayerController>();

            // Update the text fields with the player's info
            if (playerNameText != null)
            {
                // We'll use their Client ID as their name for now.
                playerNameText.text = $"Player {client.ClientId}";
            }

            if (playerScoreText != null && playerController != null)
            {
                // Read the score from the synced NetworkVariable.
                playerScoreText.text = playerController.PlayerScore.Value.ToString();
            }
        }
    }
}