using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // Required for the VideoPlayer

public class IntroController : MonoBehaviour
{
    // A reference to our VideoPlayer component
    [SerializeField] private VideoPlayer videoPlayer;

    // The name of the scene to load after the intro
    [SerializeField] private string sceneToLoad = "LobbyScene";

    // A flag to prevent loading the scene multiple times
    private bool isLoading = false;

    private void Start()
    {
        // This is the clean way to know when the video has finished.
        // We subscribe our LoadNextScene function to the videoPlayer's loopPointReached event.
        videoPlayer.loopPointReached += LoadNextScene;
    }

    private void Update()
    {
        // Allow the player to skip the intro by pressing any key or clicking the mouse.
        if (Input.anyKeyDown)
        {
            Debug.Log("Skip detected, loading next scene.");
            LoadNextScene(videoPlayer); // We pass the videoPlayer just to match the event signature
        }
    }

    // This function will be called either when the video finishes or when the player skips.
    public void LoadNextScene(VideoPlayer vp)
    {
        // Use a flag to ensure we only try to load the scene once.
        if (!isLoading)
        {
            isLoading = true;
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}