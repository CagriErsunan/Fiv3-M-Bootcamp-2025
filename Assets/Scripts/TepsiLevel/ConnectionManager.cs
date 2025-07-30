using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private string gameSceneName = "level_cambaz_tepsisi";

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    private void OnHostClicked()
    {
        // 1. Host'u başlat
        NetworkManager.Singleton.StartHost();

        // 2. Sahneyi yükle (Server tarafı yapar)
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    private void OnClientClicked()
    {
        // 1. Client başlat (Scene Load server'dan gelecek)
        NetworkManager.Singleton.StartClient();
    }
}
