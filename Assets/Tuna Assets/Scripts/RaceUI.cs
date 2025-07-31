using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace Kart.Race
{
    public class RaceUI : NetworkBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI lapText;
        public TextMeshProUGUI positionText;
        public GameObject scoreboardPanel;
        public TextMeshProUGUI scoreboardText;
        public Image itemImage; // ✅ Yeni eklendi

        private PlayerLapTracker lapTracker;
        private Kart.Items.KartInventory inventory;

        private void Start()
        {
            lapTracker = GetComponent<PlayerLapTracker>();
            inventory = GetComponent<Kart.Items.KartInventory>();

            if (!IsOwner)
            {
                lapText.gameObject.SetActive(false);
                positionText.gameObject.SetActive(false);
                itemImage.gameObject.SetActive(false);
            }

            if (scoreboardPanel != null)
                scoreboardPanel.SetActive(false);

            // ✅ Item değiştiğinde UI'yi güncelle
            if (inventory != null)
            {
                inventory.OnItemChanged += UpdateItemUI;
            }
        }

        private void Update()
        {
            if (!IsOwner || lapTracker == null) return;

            lapText.text = $"Lap {lapTracker.Lap}/{lapTracker.totalLaps}";

            int position = RaceManager.Singleton.GetPlayerPosition(OwnerClientId);
            positionText.text = position > 0 ? $"Position: {position}{GetOrdinal(position)}" : "Racing...";
        }

        private void UpdateItemUI(Kart.Items.KartItemSO item)
        {
            if (itemImage == null) return;

            if (item != null && item.icon != null)
            {
                itemImage.sprite = item.icon;
                itemImage.enabled = true;
            }
            else
            {
                itemImage.sprite = null;
                itemImage.enabled = false;
            }
        }

        public void ShowScoreboard(string finalScores)
        {
            if (!IsOwner) return;
            scoreboardPanel.SetActive(true);
            scoreboardText.text = finalScores;
        }

        private string GetOrdinal(int num)
        {
            return num switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }
    }
}
