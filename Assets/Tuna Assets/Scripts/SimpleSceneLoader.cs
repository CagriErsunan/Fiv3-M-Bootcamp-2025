using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader : MonoBehaviour
{
    // Build Settings'teki sahne indexini buraya ver
    public int sceneIndexToLoad = 1;

    // Bu fonksiyonu OnClick'te �a��r
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneIndexToLoad);
    }
}
