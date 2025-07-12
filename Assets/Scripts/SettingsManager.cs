using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Linq;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject volumeSlider;
    [SerializeField] private GameObject fullScreenToggle;
    [SerializeField] private GameObject resolutionDropdown;
   // [SerializeField] private GameObject graphicsQualityDropdown;

    Resolution[] resolutions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        SetMasterVolume(savedVolume);
        volumeSlider.GetComponent<Slider>().value = savedVolume;
        volumeSlider.GetComponent<Slider>().onValueChanged.AddListener(SetMasterVolume);

        // TAM EKRAN AYARINI YÜKLEME
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        SetFullScreen(savedFullscreen);
        fullScreenToggle.GetComponent<Toggle>().isOn = savedFullscreen;
        fullScreenToggle.GetComponent<Toggle>().onValueChanged.AddListener(SetFullScreen);

        // ÇÖZÜNÜRLÜKLERİ TESPİT ETME VE DROPDOWN'A DOLDURMA
       /* resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().ToArray();
        resolutionDropdown.GetComponent<Dropdown>().ClearOptions(); // Dropdown'ı temizle

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            // Mevcut çözünürlükle eşleşen index'i bul
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.GetComponent<Dropdown>().AddOptions(options);

        // Kaydedilmiş çözünürlüğü yükle veya mevcut olanı kullan
        int savedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        resolutionDropdown.GetComponent<Dropdown>().value = savedResolutionIndex;
        resolutionDropdown.GetComponent<Dropdown>().RefreshShownValue();
        SetResolution(savedResolutionIndex); // Oyunu kaydedilmiş çözünürlükte başlat

        resolutionDropdown.GetComponent<Dropdown>().onValueChanged.AddListener(SetResolution); */
    
        
    }
    private void SetMasterVolume(float volume)
    {
        // Example: Set the AudioListener volume (replace with your own logic if needed)
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        PlayerPrefs.SetInt("FullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.Save();
    }
}