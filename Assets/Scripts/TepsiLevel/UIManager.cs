using UnityEngine;
using UnityEngine.UI; // UI elementleri için
using TMPro; // TextMeshPro kullanıyorsanız

public class UIManager : MonoBehaviour
{
    // Singleton: Diğer script'lerin bu UIManager'a kolayca erişmesini sağlar.
    public static UIManager Instance { get; private set; }

    [Header("Puan Ekranı Elementleri")]
    public GameObject scoreScreenPanel; // Inspector'dan sürükleyeceğiniz panel
    public TMP_Text reasonText; // Kazananı/sebebi yazacak text
    public TMP_Text scoresText; // Puanları yazacak text

    public TMP_Text timerText;

    void Awake()
    {
        // Singleton'ı ayarla
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
{
    if (GameManager.Instance != null && timerText != null)
    {
        // GameManager'daki güncel zamanı al ve ekrana yazdır.
        timerText.text = "Kalan Süre: " + Mathf.Ceil(GameManager.Instance.roundTimer.Value).ToString();
    }
}

    
    public void ShowScoreScreen(string message, string scores)
    {
        // Paneli görünür yap
        scoreScreenPanel.SetActive(true);

        // Metinleri GameManager'dan gelen bilgilerle doldur
        //reasonText.text = message;
        //scoresText.text = scores;
    }
}