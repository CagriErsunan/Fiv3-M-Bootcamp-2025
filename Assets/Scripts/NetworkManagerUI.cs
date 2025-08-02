using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// This is now a simple MonoBehaviour. It does not need to be a NetworkBehaviour.
public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private GameObject buttonsPanel; 
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(() => {
            GameData.CurrentRound = 0;
            Debug.Log("HOSTING...");
            NetworkManager.Singleton.StartHost();
            // After clicking, hide the buttons immediately.
            buttonsPanel.SetActive(false);
        });

        clientButton.onClick.AddListener(() =>
        {
            Debug.Log("JOINING AS CLIENT...");

            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();

            // IP adresi elle girilebilir veya bir input field'dan alınabilir
            transport.ConnectionData.Address = "192.168.1.10"; // ← Host'un yerel IP'si
            transport.ConnectionData.Port = 7777;              // default 7777 (istersen değiştirebilirsin)

            NetworkManager.Singleton.StartClient();

            buttonsPanel.SetActive(false);
        });
        exitButton.onClick.AddListener(OnExitGameClicked);
    }
    
    private void OnExitGameClicked()
    {
        Debug.Log("Player has clicked Exit Game.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void ServerLoadScene(string sceneName)
    {
        // This can only be called by the server.
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log($"Persistent Scene Loader is loading scene: {sceneName}");
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}