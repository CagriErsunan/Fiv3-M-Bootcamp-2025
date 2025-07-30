using Unity.Netcode;
using UnityEngine;

// Bu script, oyunun build alınmış versiyonunda yer almayacak,
// sadece Unity Editor içinde çalışacaktır.
#if UNITY_EDITOR

public class NetworkTestStarter : MonoBehaviour
{
    void Start()
    {
        // Eğer zaten bir ağa bağlıysak (menüden geldiysek) hiçbir şey yapma.
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsConnectedClient)
        {
            return;
        }

        // Eğer bağlı değilsek ve editörde "Play" tuşuna basmışsak,
        // otomatik olarak bir "Host" başlat.
        Debug.LogWarning("!!! OTOMATİK TEST MODU: HOST BAŞLATILDI !!!");
        NetworkManager.Singleton.StartHost();
    }
}

#endif