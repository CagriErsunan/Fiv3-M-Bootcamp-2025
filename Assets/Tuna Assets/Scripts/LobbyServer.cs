using Unity.Netcode;
using UnityEngine;

// Bunu bir boþ GameObject'e ekleyebilirsin, sahneden silme!
public class LobbyServer : MonoBehaviour
{
    public static LobbyServer Instance;
    public string LobbyName { get; private set; } = "";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void SetLobbyName(string lobbyName)
    {
        LobbyName = lobbyName;
    }

    void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
    }

    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest req, NetworkManager.ConnectionApprovalResponse resp)
    {
        string incomingLobbyName = System.Text.Encoding.ASCII.GetString(req.Payload);
        bool approved = (incomingLobbyName == LobbyName);

        resp.Approved = approved;
        resp.CreatePlayerObject = approved;
        resp.Pending = false;
    }
}
