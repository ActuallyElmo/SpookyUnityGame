using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings instance;

    // Define the file name
    private string saveFileName = "playerSettings.json";

    [Header("Mixer References")]
    [SerializeField] AudioMixer audioMixer;

    [SerializeField] AudioMixerGroup musicMixerGroup;
    [SerializeField] AudioMixerGroup soundsMixerGroup;

    private void setAudioMixerGroupVolume(AudioMixerGroup audioMixerGroup, int volume)
    {
        float sliderValue = volume / 100f;

        float dBVolume = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;

        audioMixer.SetFloat(audioMixerGroup.name, dBVolume);
        Debug.Log("Set " + dBVolume + " to " + audioMixerGroup.name);
    }

    private void UpdateAudioSettings()
    {
        setAudioMixerGroupVolume(musicMixerGroup, musicVolume);
        setAudioMixerGroupVolume(soundsMixerGroup, soundsVolume);
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            // Auto-load settings when the game starts
            LoadPlayerSettings();

        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        UpdateAudioSettings();
    }

    // Default settings
    [Range(0, 100)]
    public int musicVolume = 50;
    [Range(0, 100)]
    public int soundsVolume = 50;

    public void LoadPlayerSettings()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            JsonUtility.FromJsonOverwrite(json, this);

            Debug.Log("Settings loaded from: " + path);
        }
        else
        {
            Debug.Log("No player settings file found, using default settings.");
        }
    }

    public void SavePlayerSettings()
    {
        string json = JsonUtility.ToJson(this, true);

        string path = Path.Combine(Application.persistentDataPath, saveFileName);

        File.WriteAllText(path, json);

        Debug.Log("Settings saved to: " + path);
    }

}