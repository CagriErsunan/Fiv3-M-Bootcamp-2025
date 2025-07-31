using Unity.Netcode;
using UnityEngine;

namespace Kart.Race
{
    public class PlayerLapTracker : NetworkBehaviour
    {
        [Header("Lap Settings")]
        public int totalLaps = 3;
        public Transform[] checkpoints; // 1-6 �eklinde s�rayla inspector�a s�r�kle

        private int currentCheckpointIndex = 0; // �u an hangi checkpointi bekliyoruz
        private NetworkVariable<int> currentLap = new NetworkVariable<int>(0);

        public int Lap => currentLap.Value;

        private void OnTriggerEnter(Collider other)
        {
            // Checkpoint mi?
            int index = GetCheckpointIndex(other.transform);
            if (index == -1) return;

            // S�radaki checkpoint mi?
            if (index == currentCheckpointIndex)
            {
                currentCheckpointIndex++;

                // 6'y� ge�tiysek (lap tamamland�)
                if (currentCheckpointIndex >= checkpoints.Length)
                {
                    currentCheckpointIndex = 0;
                    currentLap.Value++;

                    Debug.Log($"{OwnerClientId} tamamladı: Lap {currentLap.Value}/{totalLaps}");

                    if (currentLap.Value >= totalLaps)
                    {
                        RaceManager.Singleton.FinishRaceServerRpc(OwnerClientId);
                        DisableKartControl();
                    }
                }

            }
        }

        private int GetCheckpointIndex(Transform checkpoint)
        {
            for (int i = 0; i < checkpoints.Length; i++)
            {
                if (checkpoints[i] == checkpoint)
                    return i;
            }
            return -1;
        }
        private void DisableKartControl()
        {
            KartController kart = GetComponent<KartController>();
            if (kart != null)
            {
                kart.enabled = false; // Oyuncu inputunu kapat
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero; // Dursun
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log("Race Finished! Player disabled.");
        }

    }
}
