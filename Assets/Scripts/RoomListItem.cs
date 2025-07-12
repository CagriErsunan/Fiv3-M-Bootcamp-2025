using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText; // Butonun üzerindeki yazıyı Inspector'dan buraya sürükleyeceğiz
    private RoomInfo roomInfo;

    public void SetUp(RoomInfo info)
    {
        roomInfo = info;
        // Butonun metnini oda bilgileriyle güncelle: Örn: "Can'ın Odası [2/5]"
        roomNameText.text = info.Name + " [" + info.PlayerCount + "/" + info.MaxPlayers + "]";
    }

    // Bu fonksiyon butona tıklandığında çalışacak
    public void OnClick()
    {
        // LobbyManager'daki JoinRoom fonksiyonunu çağırarak bu odaya katıl
       LobbyManager.Instance.JoinRoomByName(roomInfo.Name);
    }
}