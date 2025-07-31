using Unity.Netcode;
using UnityEngine;

// Bu class, NetworkManager objesine eklenmeli!
public class NetworkManagerKurulum : MonoBehaviour
{
    // Server'da oda ad�n� saklamak i�in
    public static string lobbyName = "";

    void Start()
    {
        // Sadece server olarak ba�larsa onay callback'i ekle
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    // Server olarak lobby ba�lat�rken bir yerden lobby ad�n� al ve ata!
    public static void SetLobbyName(string name)
    {
        lobbyName = name;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Client'tan gelen connectionData'y� (oda ad�n�) oku
        string incomingLobbyName = System.Text.Encoding.ASCII.GetString(request.Payload);

        // Oda adlar�n� kar��la�t�r (server'�n saklad��� ile gelen ayn� m�)
        bool approved = (incomingLobbyName == lobbyName);

        response.Approved = approved;
        response.CreatePlayerObject = approved;
        response.Pending = false;
    }
}
