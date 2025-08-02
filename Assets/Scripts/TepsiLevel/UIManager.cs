using UnityEngine;
using UnityEngine.UI; // UI elementleri için
using TMPro; // TextMeshPro kullanıyorsanız

public class UIManagerTepsi : MonoBehaviour
{
    // Singleton: Diğer script'lerin bu UIManager'a kolayca erişmesini sağlar.
    public static UIManagerTepsi InstanceUI { get; private set; }

    [Header("Puan Ekranı Elementleri")]
    public GameObject scoreScreenPanel; // Inspector'dan sürükleyeceğiniz panel
    public TMP_Text reasonText; // Kazananı/sebebi yazacak text
    public TMP_Text scoresText; // Puanları yazacak text

    public TMP_Text timerText;

    void Awake()
    {
        // Singleton'ı ayarla
        if (InstanceUI != null && InstanceUI != this)
        {
            Destroy(gameObject);
        }
        else
        {
            InstanceUI = this;
        }
    }

    void Update()
{
    if (GameManagerTepsi.InstanceUI != null && timerText != null)
    {
        // GameManager'daki güncel zamanı al ve ekrana yazdır.
        timerText.text = "Kalan Süre: " + Mathf.Ceil(GameManagerTepsi.InstanceUI.roundTimer.Value).ToString();
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