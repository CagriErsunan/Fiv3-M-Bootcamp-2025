using UnityEngine;

public class EffectSelfDestruct : MonoBehaviour
{
    [Tooltip("How many seconds this effect will live before destroying itself.")]
    public float lifetime = 5.0f;

    void Start()
    {
        // Start the countdown to destruction as soon as the object is created.
        Destroy(gameObject, lifetime);
    }
}