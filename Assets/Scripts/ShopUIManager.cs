using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro; // Make sure you have this

public class ShopUIManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CosmeticDatabase cosmeticDatabase;
    [Header("Main Panel")]
    [SerializeField] private GameObject mainShopPanel;
    [SerializeField] private Button toggleShopButton;
    [Header("UI Containers")]
    [SerializeField] private Transform hatsContainer;    // Drag Hats_Content here
    [SerializeField] private Transform pantsContainer;   // Drag Pants_Content here
    [SerializeField] private Transform shoesContainer;   // Drag Shoes_Content here
    [SerializeField] private Transform tshirtsContainer;    // Drag Hats_Content here
    [SerializeField] private Transform costumesContainer;   // Drag Pants_Content here
    [SerializeField] private Transform glassesContainer;
    [SerializeField] private Transform facesContainer;
    [SerializeField] private Transform glovesContainer;


    [Header("UI Panels")]
    [SerializeField] private GameObject hatsPanel;        // Drag Hats_ScrollView here
    [SerializeField] private GameObject pantsPanel;       // Drag Pants_ScrollView here
    [SerializeField] private GameObject shoesPanel;
    [SerializeField] private GameObject tshirtsPanel;        // Drag Hats_ScrollView here
    [SerializeField] private GameObject costumesPanel;       // Drag Pants_ScrollView here
    [SerializeField] private GameObject glassesPanel;
    [SerializeField] private GameObject facesPanel;        // Drag Hats_ScrollView here
    [SerializeField] private GameObject glovesPanel;       // Drag Pants_ScrollView here
         // Drag Shoes_ScrollView here// Drag Shoes_ScrollView here// Drag Shoes_ScrollView here

    [Header("UI Buttons")]
    [SerializeField] private Button hatsTabButton;
    [SerializeField] private Button pantsTabButton;
    [SerializeField] private Button shoesTabButton;
    [SerializeField] private Button tshirtsTabButton;
    [SerializeField] private Button costumesTabButton;
    [SerializeField] private Button glassesTabButton;
    [SerializeField] private Button facesTabButton;
    [SerializeField] private Button glovesTabButton;
    [SerializeField] private GameObject itemButtonPrefab; // Your ShopItemButton_Template prefab

    void Start()
    {
        if (toggleShopButton != null)
        {
            toggleShopButton.onClick.AddListener(ToggleShopPanel);
        }
        // --- Hook up the tab buttons ---
        hatsTabButton.onClick.AddListener(() => SelectTab(hatsPanel));
        pantsTabButton.onClick.AddListener(() => SelectTab(pantsPanel));
        shoesTabButton.onClick.AddListener(() => SelectTab(shoesPanel));
        tshirtsTabButton.onClick.AddListener(() => SelectTab(tshirtsPanel));
        costumesTabButton.onClick.AddListener(() => SelectTab(costumesPanel));
        glassesTabButton.onClick.AddListener(() => SelectTab(glassesPanel));
        facesTabButton.onClick.AddListener(() => SelectTab(facesPanel));
        glovesTabButton.onClick.AddListener(() => SelectTab(glovesPanel));
        

        // Populate all the shop sections
        PopulateShop();

        // Select the hats tab by default when the game starts
        SelectTab(hatsPanel);
        mainShopPanel.SetActive(false);
    }
    public void ToggleShopPanel()
    {
        // If the main panel exists...
        if (mainShopPanel != null)
        {
            // ...flip its current state.
            bool isCurrentlyActive = mainShopPanel.activeSelf;
            mainShopPanel.SetActive(!isCurrentlyActive);

            // If we just turned the panel ON, default to the first tab.
            if (!isCurrentlyActive)
            {
                SelectTab(hatsPanel);
            }
        }
    }
        // This function shows one panel and hides the others
        void SelectTab(GameObject panelToShow)
    {
        hatsPanel.SetActive(panelToShow == hatsPanel);
        pantsPanel.SetActive(panelToShow == pantsPanel);
        shoesPanel.SetActive(panelToShow == shoesPanel);
        tshirtsPanel.SetActive(panelToShow == tshirtsPanel);
        costumesPanel.SetActive(panelToShow == costumesPanel);
        glassesPanel.SetActive(panelToShow == glassesPanel);
        facesPanel.SetActive(panelToShow == facesPanel);
        glovesPanel.SetActive(panelToShow == glovesPanel);
        
    }

    void PopulateShop()
    {
        // --- Populate Hats ---
        // Make sure the container is empty before adding new buttons
        foreach (Transform child in hatsContainer) Destroy(child.gameObject);

        for (int i = 0; i < cosmeticDatabase.allHats.Count; i++)
        {
            CosmeticItem item = cosmeticDatabase.allHats[i];
            int itemIndex = i;
            GameObject buttonGO = Instantiate(itemButtonPrefab, hatsContainer);

            // Find the TextMeshPro component to set the name
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = item.itemName;

            buttonGO.GetComponent<Button>().onClick.AddListener(() => {
                OnCosmeticButtonClicked(itemIndex, CosmeticType.Hat);
            });
        }

        // --- Populate Pants ---
        foreach (Transform child in pantsContainer) Destroy(child.gameObject);

        for (int i = 0; i < cosmeticDatabase.allPants.Count; i++)
        {
            CosmeticItem item = cosmeticDatabase.allPants[i];
            int itemIndex = i;
            GameObject buttonGO = Instantiate(itemButtonPrefab, pantsContainer);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = item.itemName;
            buttonGO.GetComponent<Button>().onClick.AddListener(() => {
                OnCosmeticButtonClicked(itemIndex, CosmeticType.Pants);
            });
        }

        // --- Populate Shoes ---
        foreach (Transform child in shoesContainer) Destroy(child.gameObject);

        for (int i = 0; i < cosmeticDatabase.allShoes.Count; i++)
        {
            CosmeticItem item = cosmeticDatabase.allShoes[i];
            int itemIndex = i;
            GameObject buttonGO = Instantiate(itemButtonPrefab, shoesContainer);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = item.itemName;
            buttonGO.GetComponent<Button>().onClick.AddListener(() => {
                OnCosmeticButtonClicked(itemIndex, CosmeticType.Shoes);
            });
        }
        // --- Populate Shoes ---
        foreach (Transform child in tshirtsContainer) Destroy(child.gameObject);

        for (int i = 0; i < cosmeticDatabase.allTshirts.Count; i++)
        {
            CosmeticItem item = cosmeticDatabase.allTshirts[i];
            int itemIndex = i;
            GameObject buttonGO = Instantiate(itemButtonPrefab, tshirtsContainer);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = item.itemName;
            buttonGO.GetComponent<Button>().onClick.AddListener(() => {
                OnCosmeticButtonClicked(itemIndex, CosmeticType.Tshirt);
            });
        }
        // --- Populate Shoes ---
        foreach (Transform child in costumesContainer) Destroy(child.gameObject);

        for (int i = 0; i < cosmeticDatabase.allCostumes.Count; i++)
        {
            CosmeticItem item = cosmeticDatabase.allCostumes[i];
            int itemIndex = i;
            GameObject buttonGO = Instantiate(itemButtonPrefab, costumesContainer);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = item.itemName;
            buttonGO.GetComponent<Button>().onClick.AddListener(() => {
                OnCosmeticButtonClicked(itemIndex, CosmeticType.Costume);
            });
        }
        // --- Populate Shoes ---
        foreach (Transform child in glassesContainer) Destroy(child.gameObject);

        for (int i = 0; i < cosmeticDatabase.allGlasses.Count; i++)
        {
            CosmeticItem item = cosmeticDatabase.allGlasses[i];
            int itemIndex = i;
            GameObject buttonGO = Instantiate(itemButtonPrefab, glassesContainer);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = item.itemName;
            buttonGO.GetComponent<Button>().onClick.AddListener(() => {
                OnCosmeticButtonClicked(itemIndex, CosmeticType.Glasses);
            });
        }
        // --- Populate Shoes ---
        foreach (Transform child in facesContainer) Destroy(child.gameObject);

        for (int i = 0; i < cosmeticDatabase.allFaces.Count; i++)
        {
            CosmeticItem item = cosmeticDatabase.allFaces[i];
            int itemIndex = i;
            GameObject buttonGO = Instantiate(itemButtonPrefab, facesContainer);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = item.itemName;
            buttonGO.GetComponent<Button>().onClick.AddListener(() => {
                OnCosmeticButtonClicked(itemIndex, CosmeticType.Face);
            });
        }
        // --- Populate Shoes ---
        foreach (Transform child in glovesContainer) Destroy(child.gameObject);

        for (int i = 0; i < cosmeticDatabase.allGloves.Count; i++)
        {
            CosmeticItem item = cosmeticDatabase.allGloves[i];
            int itemIndex = i;
            GameObject buttonGO = Instantiate(itemButtonPrefab, glovesContainer);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = item.itemName;
            buttonGO.GetComponent<Button>().onClick.AddListener(() => {
                OnCosmeticButtonClicked(itemIndex, CosmeticType.Gloves);
            });
        }
    }

    void OnCosmeticButtonClicked(int itemIndex, CosmeticType type)
    {
        if (NetworkManager.Singleton.LocalClient?.PlayerObject == null)
        {
            Debug.LogWarning("Player object not ready yet.");
            return;
        }

        PlayerController localPlayerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();

        if (localPlayerController != null)
        {
            localPlayerController.RequestCosmeticChangeServerRpc(itemIndex, type);
        }
    }
}