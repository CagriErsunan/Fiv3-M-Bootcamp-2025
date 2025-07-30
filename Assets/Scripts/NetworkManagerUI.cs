using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// This is now a simple MonoBehaviour. It does not need to be a NetworkBehaviour.
public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private GameObject buttonsPanel; // The panel holding the buttons

    private void Awake()
    {
        hostButton.onClick.AddListener(() => {
            Debug.Log("HOSTING...");
            NetworkManager.Singleton.StartHost();
            // After clicking, hide the buttons immediately.
            buttonsPanel.SetActive(false);
        });

        clientButton.onClick.AddListener(() => {
            Debug.Log("JOINING AS CLIENT...");
            NetworkManager.Singleton.StartClient();
            // After clicking, hide the buttons immediately.
            buttonsPanel.SetActive(false);
        });
    }
    // In NetworkManagerUI.cs

    public void ServerLoadScene(string sceneName)
    {
        // This can only be called by the server.
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log($"Persistent Scene Loader is loading scene: {sceneName}");
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}