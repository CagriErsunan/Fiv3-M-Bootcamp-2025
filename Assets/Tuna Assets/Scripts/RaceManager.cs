using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Kart.Race
{
    public class RaceManager : NetworkBehaviour
    {
        public static RaceManager Singleton;

        [Header("Race Settings")]
        public int totalLaps = 3;
        public int maxPlayers = 5; // puan tablosu için

        private List<ulong> finishOrder = new List<ulong>(); // sıralamayı tutar
        private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();

        private void Awake() => Singleton = this;

        [ServerRpc(RequireOwnership = false)]
        public void FinishRaceServerRpc(ulong playerId)
        {
            if (finishOrder.Contains(playerId)) return; // Aynı oyuncu iki kez bitiremez

            finishOrder.Add(playerId);
            int position = finishOrder.Count;

            int score = CalculateScore(position);
            playerScores[playerId] = score;

            Debug.Log($"Player {playerId} finished! Pos {position}, Score {score}");

            // Scoreboard'u tüm clientlere gönder
            string finalBoard = GetScoreboard();
            FinishRaceClientRpc(playerId, position, score, finalBoard);
        }

        [ClientRpc]
        private void FinishRaceClientRpc(ulong playerId, int position, int score, string finalBoard)
        {
            Debug.Log($"Player {playerId} finished. Pos {position}, Score {score}");

            // Sadece kendi oyuncusunda scoreboard göster
            foreach (var kart in FindObjectsOfType<RaceUI>())
            {
                if (kart.IsOwner)
                {
                    kart.ShowScoreboard(finalBoard);
                }
            }
        }

        private int CalculateScore(int position)
        {
            switch (position)
            {
                case 1: return 5;
                case 2: return 3;
                case 3: return 2;
                case 4: return 1;
                default: return 0;
            }
        }

        public int GetPlayerPosition(ulong playerId)
        {
            int index = finishOrder.IndexOf(playerId);
            return index >= 0 ? index + 1 : 0;
        }

        public string GetScoreboard()
        {
            string result = "Final Scores:\n";
            for (int i = 0; i < finishOrder.Count; i++)
            {
                ulong playerId = finishOrder[i];
                int score = playerScores[playerId];
                result += $"{i + 1}. Player {playerId} - {score} pts\n";
            }
            return result;
        }
    }
}
