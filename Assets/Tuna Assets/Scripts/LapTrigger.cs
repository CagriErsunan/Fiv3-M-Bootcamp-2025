using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LapTrigger : MonoBehaviour
{
    [SerializeField] private int lapsToWin = 3; // 3 tur kazanma
    [SerializeField] private string nextSceneName = "RaceResultScene"; // Kazanıldığında geçilecek sahne

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Kart.KartController>(out var kart))
        {
            if (!kart.IsOwner) return; // Sadece owner sayacak

            kart.AddScoreServerRpc(1);

            Debug.Log($"Player {kart.OwnerClientId} Lap: {kart.PlayerScore.Value}");

            // Check lap on server
            CheckWinConditionServerRpc(kart.NetworkObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckWinConditionServerRpc(ulong kartId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(kartId, out var netObj))
        {
            var kart = netObj.GetComponent<Kart.KartController>();
            if (kart.PlayerScore.Value >= lapsToWin)
            {
                Debug.Log($"Player {kart.OwnerClientId} WON THE RACE!");
                EndRaceClientRpc(kart.OwnerClientId);
            }
        }
    }

    [ClientRpc]
    private void EndRaceClientRpc(ulong winnerId)
    {
        Debug.Log($"Race Finished! Winner: Player {winnerId}");

        // 🏁 Sahne geçişi sadece server yapar
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }
}
