using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        // Cargar el volumen guardado al iniciar
        float savedVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        volumeSlider.value = savedVolume;
        // Aquí puedes aplicar el volumen a tu AudioManager si lo tienes
    }

    public void OnVolumeChanged(float value)
    {
        // Guardar el nuevo volumen cuando el usuario cambie el slider
        PlayerPrefs.SetFloat("musicVolume", value);
        PlayerPrefs.Save();
        // Aquí puedes aplicar el volumen a tu AudioManager si lo tienes
    }
}