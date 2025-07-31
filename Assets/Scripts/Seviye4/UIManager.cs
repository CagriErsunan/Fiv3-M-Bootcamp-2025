using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject winnerPanel;

    [Header("Game Over Panel Children")]
    public TMP_Text gameOverText;
    public Button retryButton;

    [Header("Winner Panel Children")]
    public TMP_Text winnerText;
    public Button nextSceneButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        gameOverPanel?.SetActive(false);
        winnerPanel?.SetActive(false);
    }

    public void ShowGameOver(bool isWinner)
    {
        if (isWinner)
        {
            winnerPanel?.SetActive(true);
            gameOverPanel?.SetActive(false);

            if (winnerText != null)
                winnerText.text = "YOU WON!";

            nextSceneButton?.gameObject.SetActive(true);
        }
        else
        {
            gameOverPanel?.SetActive(true);
            winnerPanel?.SetActive(false);

            if (gameOverText != null)
                gameOverText.text = "Game Over!";

            retryButton?.gameObject.SetActive(true);
        }
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToNextScene()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            Debug.LogWarning("Son sahnedesin.");
    }
}
