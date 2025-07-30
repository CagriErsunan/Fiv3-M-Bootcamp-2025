using UnityEngine;

// This is a simple Singleton to hold UI references.
public class UIManager : MonoBehaviour
{
    // A static instance so any script can easily access it.
    public static UIManager Instance { get; private set; }

    // A public field for our Haste Skill UI.
    public HasteSkillUIController HasteSkillUI;

    private void Awake()
    {
        // Set up the Singleton instance.
        Instance = this;
    }
}