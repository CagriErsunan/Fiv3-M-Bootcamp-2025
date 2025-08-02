using UnityEngine;
using UnityEngine.UI;

public class VotingUI : MonoBehaviour
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    // We'll add references to all possible GameManagers here.
    private GameManager gameManager;
    private GameManager_Zombie gameManagerZombie;
    private GameManager_BossArena gameManagerBoss;
    private GameManager_Tepsi gameManagerTepsi; // For the Tepsi level

    // When the voting panel is enabled, we find our manager.
    [System.Obsolete]
    private void OnEnable()
    {
        // Try to find each type of manager. Only one will exist in any given scene.
        gameManager = FindObjectOfType<GameManager>();
        gameManagerZombie = FindObjectOfType<GameManager_Zombie>();
        gameManagerBoss = FindObjectOfType<GameManager_BossArena>();
        gameManagerTepsi = FindObjectOfType<GameManager_Tepsi>();
    }

    private void Start()
    {
        yesButton.onClick.AddListener(() => SubmitVote(true));
        noButton.onClick.AddListener(() => SubmitVote(false));
    }

    private void SubmitVote(bool vote)
    {
        // Now, we just check which manager we found.
        if (gameManager != null)
        {
            gameManager.ReceiveVote(vote);
        }
        else if (gameManagerZombie != null)
        {
            gameManagerZombie.ReceiveVote(vote);
        }
        else if (gameManagerBoss != null)
        {
            gameManagerBoss.ReceiveVote(vote);
        }
        else if (gameManagerTepsi != null)
        {
            gameManagerTepsi.ReceiveVote(vote);
        }
        else
        {
            // This is the error you were seeing.
            Debug.LogError("VotingUI could not find any active GameManager in the scene when the button was clicked!");
        }

        // Disable buttons.
        yesButton.interactable = false;
        noButton.interactable = false;
    }
}