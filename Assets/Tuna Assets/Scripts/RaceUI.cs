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
        public Image itemImage; // Elde edilen item ikonunu gösterir

        private PlayerLapTracker lapTracker;
        private Kart.Items.KartInventory inventory;

        private void Start()
        {
            lapTracker = GetComponent<PlayerLapTracker>();
            inventory = GetComponent<Kart.Items.KartInventory>();

            // Sadece owner kendi UI'sini görsün
            if (!IsOwner)
            {
                if (lapText != null) lapText.gameObject.SetActive(false);
                if (positionText != null) positionText.gameObject.SetActive(false);
                if (itemImage != null) itemImage.gameObject.SetActive(false);
            }

            if (scoreboardPanel != null)
                scoreboardPanel.SetActive(false);

            // Item değiştiğinde UI güncelle
            if (inventory != null)
            {
                inventory.OnItemChanged += UpdateItemUI;
            }
        }

        private void Update()
        {
            if (!IsOwner || lapTracker == null) return;

            // Lap UI güncelle
            lapText.text = $"Lap {lapTracker.Lap}/{lapTracker.totalLaps}";

            // Pozisyonu RaceManager’dan al
            if (RaceManager.Singleton != null)
            {
                int position = RaceManager.Singleton.GetPlayerPosition(OwnerClientId);
                positionText.text = position > 0
                    ? $"Position: {position}{GetOrdinal(position)}"
                    : "Racing...";
            }
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
            if (scoreboardPanel != null) scoreboardPanel.SetActive(true);
            if (scoreboardText != null) scoreboardText.text = finalScores;
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
