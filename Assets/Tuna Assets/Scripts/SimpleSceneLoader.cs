using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader : MonoBehaviour
{
    // Build Settings'teki sahne indexini buraya ver
    public int sceneIndexToLoad = 1;

    // Bu fonksiyonu OnClick'te çaðýr
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneIndexToLoad);
    }
}
