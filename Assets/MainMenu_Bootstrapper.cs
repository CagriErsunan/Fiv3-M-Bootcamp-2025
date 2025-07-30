using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// This is a simple, NON-NETWORKED script. It's a MonoBehaviour.
public class MainMenu_Bootstrapper : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private GameObject networkHUDPrefab; // The prefab for our NetworkHUD_Object

    private void Awake()
    {
        hostButton.onClick.AddListener(() => {
            // When Host is clicked, start the host...
            NetworkManager.Singleton.StartHost();

            // ...and then MANUALLY spawn our Networked HUD object.
            // The server is the only one who can spawn objects.
            GameObject hudInstance = Instantiate(networkHUDPrefab);
            hudInstance.GetComponent<NetworkObject>().Spawn();

            // Then load the lobby scene.
            NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        });

        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
    }
}