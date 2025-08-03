using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneModeController : NetworkBehaviour
{
    [Header("Components")]
    public PlayerController playerController;
    public Kart.KartController kartController;
    public Rigidbody rb;
    public Animator animator;

    void Start()
    {
        if (!IsOwner) return;

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyMode(SceneManager.GetActiveScene().buildIndex);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsOwner) return;
        ApplyMode(scene.buildIndex);
    }

    private void ApplyMode(int sceneIndex)
    {
        // �RNEK: 0-3 normal sahneler, 5 yar�� sahnesi
        bool isRaceScene = (sceneIndex == 5);

        if (playerController != null) playerController.enabled = !isRaceScene;
        if (kartController != null) kartController.enabled = isRaceScene;

        // Fizik ayar�
        if (rb != null)
        {
            rb.useGravity = isRaceScene;
            rb.isKinematic = !isRaceScene;
        }

        // Animat�r ayar� (y�r�y�� animasyonu sadece normal modda)
        if (animator != null)
            animator.enabled = !isRaceScene;

        Debug.Log($"[SceneModeController] Mode set: {(isRaceScene ? "Race" : "Normal")} for {OwnerClientId}");
    }
}
