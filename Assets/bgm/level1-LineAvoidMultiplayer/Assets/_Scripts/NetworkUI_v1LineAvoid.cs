using Unity.Netcode;
using UnityEngine;

public class NetworkUIManager : MonoBehaviour
{
    public GameObject uiPanel;

    public void StartHost()
    {
        Debug.Log("Host başlatılıyor...");
        NetworkManager.Singleton.StartHost();
        uiPanel.SetActive(false);
    }

    public void StartClient()
    {
        Debug.Log("Client bağlanıyor...");
        NetworkManager.Singleton.StartClient();
        uiPanel.SetActive(false);
    }
}
