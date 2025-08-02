using UnityEngine;
using TMPro;

public class UIManager_Sm : MonoBehaviour
{
    public static UIManager_Sm Instance;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text resultText;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Oyuncuya oyun sonu ekranýný gösterir.
    /// </summary>
    public void ShowGameOver(bool isWinner)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (resultText != null)
            resultText.text = isWinner ? "Kazandýn!" : "Kaybettin!";

        Debug.Log(isWinner ? "[UIManager_Sm] Kazandýn!" : "[UIManager_Sm] Kaybettin!");
    }
}
