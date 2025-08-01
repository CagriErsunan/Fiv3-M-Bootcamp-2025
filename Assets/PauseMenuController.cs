using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Needed for the Dropdown
using Unity.Netcode; // Needed to disconnect from the game
using UnityEngine.SceneManagement; // Needed to load the lobby

public class PauseMenuController : MonoBehaviour
{
    public static bool IsGamePaused = false;

    [Header("Main Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Main Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button returnToLobbyButton;

    [Header("Settings UI")]
    [SerializeField] private Button backButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private Resolution[] resolutions; // To store available screen resolutions

    private void Start()
    {
        // --- Setup Listeners for Main Buttons ---
        resumeButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(OpenSettings);
        returnToLobbyButton.onClick.AddListener(ReturnToLobby);
        backButton.onClick.AddListener(CloseSettings);

        // --- Setup Listeners for Settings ---
        volumeSlider.onValueChanged.AddListener(SetVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        // Populate the resolution dropdown with the user's available resolutions
        PopulateResolutions();

        // Ensure everything is hidden at the start
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void Update()
    {
        // Listen for the Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // --- Main Menu Functions ---
    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        
        IsGamePaused = false;
        // You might want to unlock and show the cursor here if you lock it during gameplay
       // Cursor.lockState = CursorLockMode.Locked;
      //  Cursor.visible = false;
    }

    private void Pause()
    {
        pauseMenuPanel.SetActive(true);
        
        IsGamePaused = true;
        // You might want to unlock and show the cursor here
      //  Cursor.lockState = CursorLockMode.None;
       //    Cursor.visible = true;
    }

    private void OpenSettings()
    {
        settingsPanel.SetActive(true);
        // We can hide the main buttons when the settings panel is open
        resumeButton.transform.parent.gameObject.SetActive(false);
    }

    private void CloseSettings()
    {
        settingsPanel.SetActive(false);
        resumeButton.transform.parent.gameObject.SetActive(true);
    }

    private void ReturnToLobby()
    {
        // Resume time before we disconnect, otherwise scene loading can get stuck
        Time.timeScale = 1f;
        IsGamePaused = false;

        // Properly disconnect from the network session
        NetworkManager.Singleton.Shutdown();

        // Destroy the persistent NetworkManager to ensure a clean start in the lobby
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        // Load the lobby scene
        SceneManager.LoadScene("LobbyScene");
    }


    // --- Settings Functions ---
    public void SetVolume(float volume)
    {
        // Note: This requires setting up an AudioMixer. For now, we'll just log it.
        Debug.Log("Volume set to: " + volume);
        // AudioListener.volume = volume; // A simpler way to set master volume
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log("Fullscreen set to: " + isFullscreen);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        Debug.Log($"Resolution set to: {resolution.width}x{resolution.height}");
    }

    private void PopulateResolutions()
    {
        resolutions = Screen.resolutions.Select(res => new Resolution { width = res.width, height = res.height }).Distinct().ToArray();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
}