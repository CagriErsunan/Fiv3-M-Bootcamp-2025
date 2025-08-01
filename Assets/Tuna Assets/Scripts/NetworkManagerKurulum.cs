using Unity.Netcode;
using UnityEngine;

// Bu class, NetworkManager objesine eklenmeli!
public class NetworkManagerKurulum : MonoBehaviour
{
    // Server'da oda adýný saklamak için
    public static string lobbyName = "";

    void Start()
    {
        // Sadece server olarak baþlarsa onay callback'i ekle
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    // Server olarak lobby baþlatýrken bir yerden lobby adýný al ve ata!
    public static void SetLobbyName(string name)
    {
        lobbyName = name;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Client'tan gelen connectionData'yý (oda adýný) oku
        string incomingLobbyName = System.Text.Encoding.ASCII.GetString(request.Payload);

        // Oda adlarýný karþýlaþtýr (server'ýn sakladýðý ile gelen ayný mý)
        bool approved = (incomingLobbyName == lobbyName);

        response.Approved = approved;
        response.CreatePlayerObject = approved;
        response.Pending = false;
    }
}
