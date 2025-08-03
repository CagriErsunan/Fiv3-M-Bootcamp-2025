using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Kart.Race
{
    public class RaceManager : NetworkBehaviour
    {
        public static RaceManager Singleton;

        [SerializeField] private int totalLaps = 3;

        // Oyuncu -> Bitirdiği tur sayısı
        private Dictionary<ulong, int> playerLapDict = new Dictionary<ulong, int>();
        private List<ulong> finishedPlayers = new List<ulong>();

        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Oyuncu yeni bir lap tamamladı
        /// </summary>
        public void RegisterLap(ulong clientId)
        {
            if (!playerLapDict.ContainsKey(clientId))
                playerLapDict[clientId] = 0;

            playerLapDict[clientId]++;

            Debug.Log($"Player {clientId} Lap: {playerLapDict[clientId]}/{totalLaps}");

            if (playerLapDict[clientId] >= totalLaps && IsServer)
            {
                FinishRaceServerRpc(clientId);
            }
        }

        /// <summary>
        /// Oyuncunun sıralamasını döner
        /// </summary>
        public int GetPlayerPosition(ulong clientId)
        {
            if (!playerLapDict.ContainsKey(clientId))
                return 0;

            int myLap = playerLapDict[clientId];
            int position = 1;

            foreach (var kvp in playerLapDict)
            {
                if (kvp.Value > myLap)
                    position++;
            }

            return position;
        }

        /// <summary>
        /// Server: Oyuncu yarışı bitirdiğinde çağrılır
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void FinishRaceServerRpc(ulong clientId)
        {
            if (!finishedPlayers.Contains(clientId))
            {
                finishedPlayers.Add(clientId);
                Debug.Log($"Player {clientId} finished the race! Position: {finishedPlayers.Count}");

                // Eğer tüm oyuncular bitirdiyse scoreboard hazırlanır
                if (finishedPlayers.Count == NetworkManager.Singleton.ConnectedClients.Count)
                {
                    Debug.Log("All players finished the race!");
                    ShowScoreboardToAll();
                }
            }
        }

        /// <summary>
        /// Herkes bitirdiğinde scoreboard'u tüm clientlara göster
        /// </summary>
        private void ShowScoreboardToAll()
        {
            string finalScores = "🏆 FINAL RESULTS 🏆\n";

            for (int i = 0; i < finishedPlayers.Count; i++)
            {
                finalScores += $"{i + 1}. Player {finishedPlayers[i]}\n";
            }

            foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
            {
                var raceUI = client.PlayerObject.GetComponent<Kart.Race.RaceUI>();
                if (raceUI != null)
                {
                    raceUI.ShowScoreboard(finalScores);
                }
            }
        }
    }
}
