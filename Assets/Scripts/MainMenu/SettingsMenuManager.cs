using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider musicSlider;
    public Slider soundSlider;

    [Header("Mixer References")]
    [SerializeField] AudioMixer audioMixer;

    [SerializeField] AudioMixerGroup musicMixerGroup;
    [SerializeField] AudioMixerGroup soundsMixerGroup;

    
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


    private void setAudioMixerGroupVolume(AudioMixerGroup audioMixerGroup, int volume)
    {
        float sliderValue = volume / 100f;

        float dBVolume = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;

        audioMixer.SetFloat(audioMixerGroup.name, dBVolume);
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
        int musicSliderValue = Mathf.RoundToInt(musicSlider.value);
        if (musicValueText != null)
            musicValueText.text = musicSliderValue.ToString();
        
        setAudioMixerGroupVolume(musicMixerGroup, musicSliderValue);
    }

    public void OnSoundSliderChanged()
    {
        int soundsSliderValue = Mathf.RoundToInt(soundSlider.value);
        if (soundValueText != null)
            soundValueText.text = soundsSliderValue.ToString();

        setAudioMixerGroupVolume(soundsMixerGroup, soundsSliderValue);
    }

    private void UpdateVolumeLabels()
    {
        if (musicValueText != null)
            musicValueText.text = musicSlider.value.ToString();
        if (soundValueText != null)
            soundValueText.text = soundSlider.value.ToString();
    }
}