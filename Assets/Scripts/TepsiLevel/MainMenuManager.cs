using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    public void PlayGame()
    {
        // Logic to start the game
        Debug.Log("Starting Game...");
        // Load the game scene or perform any other necessary actions
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettingsPanel()
    {
        // Logic to open the settings panel
        Debug.Log("Opening Settings...");
        settingsPanel.SetActive(true);
    }

    public void CloseSettingsPanel()
    {
        // Logic to close the settings panel
        Debug.Log("Closing Settings...");
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        // Logic to quit the game
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
