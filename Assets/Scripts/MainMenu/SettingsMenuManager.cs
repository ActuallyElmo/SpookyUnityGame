using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider musicSlider;
    public Slider soundSlider;

    public TMP_Text musicValueText;
    public TMP_Text soundValueText;

    void OnEnable()
    {
        LoadUIFromSettings();
    }

    public void LoadUIFromSettings()
    {
        if (PlayerSettings.instance == null)
        {
            Debug.LogError("PlayerSettings instance not found!");
            return;
        }

        musicSlider.value = PlayerSettings.instance.musicVolume;
        soundSlider.value = PlayerSettings.instance.soundsVolume;

        UpdateVolumeLabels();
    }

    public void CloseAndSaveSettingsTab()
    {
        SaveSettingsFromUI();
        this.gameObject.SetActive(false);
    }

    public void SaveSettingsFromUI()
    {
        if (PlayerSettings.instance == null) return;

        PlayerSettings.instance.musicVolume = Mathf.RoundToInt(musicSlider.value);
        PlayerSettings.instance.soundsVolume = Mathf.RoundToInt(soundSlider.value);

        PlayerSettings.instance.SavePlayerSettings();
        
        Debug.Log("UI settings applied and saved.");
    }
    public void OnMusicSliderChanged()
    {
        if (musicValueText != null)
            musicValueText.text = Mathf.RoundToInt(musicSlider.value).ToString();
    }

    public void OnSoundSliderChanged()
    {
        if (soundValueText != null)
            soundValueText.text = Mathf.RoundToInt(soundSlider.value).ToString();
    }

    private void UpdateVolumeLabels()
    {
        if (musicValueText != null)
            musicValueText.text = musicSlider.value.ToString();
        if (soundValueText != null)
            soundValueText.text = soundSlider.value.ToString();
    }
}