using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject volumeSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        SetMasterVolume(savedVolume);
        volumeSlider.GetComponent<Slider>().value = savedVolume;

        volumeSlider.GetComponent<Slider>().onValueChanged.AddListener(SetMasterVolume);
    }
    private void SetMasterVolume(float volume)
    {
        // Example: Set the AudioListener volume (replace with your own logic if needed)
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
}