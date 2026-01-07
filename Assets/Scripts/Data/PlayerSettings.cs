using UnityEngine;
using System.IO;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings instance;

    // Define the file name
    private string saveFileName = "playerSettings.json";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            
            // Auto-load settings when the game starts
            LoadPlayerSettings(); 
        }
        else if(instance != this)
        {
            Destroy(this.gameObject);
        }
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