using UnityEngine;
// This script's only job is to make its GameObject persistent.
public class PersistentObject : MonoBehaviour
{
    void Awake() { DontDestroyOnLoad(gameObject); }
}