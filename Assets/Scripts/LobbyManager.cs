using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

// MonoBehaviourPunCallbacks sayesinde Photon'un hazır fonksiyonlarını (OnConnectedToMaster, OnJoinedRoom vb.) kullanabiliriz.
public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager Instance; // Singleton instance
    public TMP_InputField playerNameInput;
    public TMP_InputField roomNameInput;
    public TMP_Text statusText;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button connectButton;

    [Header("Oda Listesi UI")] // YENİ BÖLÜM
    public Transform roomListContent; // Odaların listeleneceği Content objesi
    public GameObject roomListItemPrefab; // Az önce oluşturduğumuz prefab

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        statusText.text = "Lütfen isminizi girin ve 'Bağlan' butonuna basın.";
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        PhotonNetwork.AutomaticallySyncScene = true;
        //PhotonNetwork.ConnectUsingSettings();
    }
    public void Connect()
    {
        if (string.IsNullOrEmpty(playerNameInput.text))
        {
            statusText.text = "Lütfen oyuncu adınızı girin.";
            return;
        }

        PhotonNetwork.NickName = playerNameInput.text;
        statusText.text = "Sunucuya bağlanılıyor...";
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
        // Connect butonunu devre dışı bırak
        connectButton.interactable = false;
    }

    // Photon sunucularına başarıyla bağlandığımızda otomatik olarak çalışır
    public override void OnConnectedToMaster()
    {
        statusText.text = "Sunucuya Bağlanıldı. Lobiye Katılınıyor...";
        // Genel lobiye katıl
        PhotonNetwork.JoinLobby();
    }

    // Lobiye katıldığımızda çalışır
    public override void OnJoinedLobby()
    {
        statusText.text = "Lobiye katıldınız! Oda oluşturabilir veya katılabilirsiniz.";
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }

    // "Oda Oluştur" butonuna basıldığında çalışacak fonksiyon
    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text) || string.IsNullOrEmpty(playerNameInput.text)) return;

        PhotonNetwork.NickName = playerNameInput.text; // Oyuncu adımızı ağ üzerinde ayarla
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 5 }; // Oda en fazla 5 kişilik olabilir
        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
        statusText.text = "'" + roomNameInput.text + "' Odası Oluşturuluyor...";
    }

    // "Odaya Katıl" butonuna basıldığında çalışacak fonksiyon
    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text) || string.IsNullOrEmpty(playerNameInput.text)) return;

        PhotonNetwork.NickName = playerNameInput.text; // Oyuncu adımızı ayarla
        PhotonNetwork.JoinRoom(roomNameInput.text);
        statusText.text = "'" + roomNameInput.text + "' Odasına Katılınıyor...";
    }

    // Bir odaya başarıyla katıldığımızda çalışır
    public override void OnJoinedRoom()
    {
        statusText.text = "Odaya Katıldınız! Oyun Başlıyor...";
        // Tüm oyuncuları oyun sahnesine yükle
        PhotonNetwork.LoadLevel("level_cambaz_tepsisi"); // Oyun sahnenizin adını buraya yazın!
    }

    // Odaya katılma başarısız olduğunda çalışır
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "Odaya Katılınamadı: " + message;
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Önce mevcut UI listesini temizle
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }

        // Gelen listeyi kendi listemize işleyelim
        for(int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            // Oda kapatılmışsa veya görünmezse listemizden çıkar
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info);
            }
            // Yoksa listemize ekle
            else
            {
                // Eğer listede yoksa ekle, varsa güncelle (bu basit örnekte direkt ekliyoruz)
                if(!cachedRoomList.Contains(info))
                    cachedRoomList.Add(info);
            }
        }

        // Son olarak, güncel listemizi UI'da göster
        foreach (RoomInfo info in cachedRoomList)
        {
            if (info.PlayerCount > 0) // Sadece içinde oyuncu olan odaları göster
            {
                GameObject newListItem = Instantiate(roomListItemPrefab, roomListContent);
                newListItem.GetComponent<RoomListItem>().SetUp(info);
            }
        }
    }


    // Buton yerine listeden tıklayarak odaya katılmak için
    public void JoinRoomByName(string name)
    {
        if (string.IsNullOrEmpty(playerNameInput.text))
        {
            statusText.text = "Lütfen önce oyuncu adınızı girin!";
            return;
        }

        PhotonNetwork.NickName = playerNameInput.text;
        PhotonNetwork.JoinRoom(name);
        statusText.text = "'" + name + "' Odasına Katılınıyor...";
    }

}