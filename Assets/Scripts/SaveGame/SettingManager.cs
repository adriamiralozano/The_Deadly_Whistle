using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        volumeSlider.value = savedVolume;
    }

    public void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("musicVolume", value);
        PlayerPrefs.Save();
    }
}