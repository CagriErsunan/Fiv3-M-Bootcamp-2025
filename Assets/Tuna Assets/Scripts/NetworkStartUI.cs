// Scripts/NetworkStartUI.cs
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Kart
{
    public class NetworkStartUI : MonoBehaviour
    {
        public Button startHostButton;
        public Button startClientButton;

        void Start()
        {
            startHostButton.onClick.AddListener(StartHost);
            startClientButton.onClick.AddListener(StartClient);
        }

        // 🔥 BUNLAR ARTIK PUBLIC!
        public void StartHost()
        {
            Debug.Log("Host başlatılıyor...");
            NetworkManager.Singleton.StartHost();
            gameObject.SetActive(false);
        }

        public void StartClient()
        {
            Debug.Log("Client başlatılıyor...");
            NetworkManager.Singleton.StartClient();
            gameObject.SetActive(false);
        }
    }
}
