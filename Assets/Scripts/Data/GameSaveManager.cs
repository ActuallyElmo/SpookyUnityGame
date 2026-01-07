using UnityEngine;
using System.IO;

[System.Serializable]
public class SavedGameData
{
    public bool hasSavedGame;
    public bool isNewSave;
    public Vector3 playerPosition;
    public Vector3 playerRotation;

    //Game Progression
    public Vector3 flashlightPosition;


    //World state
    public int[] mapDoorStates;

    public SavedGameData()
    {
        hasSavedGame = false;
        isNewSave = true;
        playerPosition = Vector3.zero;
        flashlightPosition = Vector3.zero;
        mapDoorStates = new int[0];
    }
}

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager instance;

    // This variable holds the actual data
    public SavedGameData currentSaveData;

    private string saveFileName = "savegame.json";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if(instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    public bool VerifyForSavedGame()
    {
        LoadGame();

        return currentSaveData.hasSavedGame;
    }

    public void LoadGame()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            currentSaveData = JsonUtility.FromJson<SavedGameData>(json);
            
            Debug.Log("Loading Saved Game");
        }
        else
        {
            Debug.Log("No save file found.");
            currentSaveData = new SavedGameData();
            currentSaveData.hasSavedGame = false; 
        }
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(currentSaveData, true);
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        
        File.WriteAllText(path, json);
        
        Debug.Log("Game Saved to: " + path);
    }

    public void StartNewGame()
    {
        currentSaveData = new SavedGameData(); 
        
        currentSaveData.hasSavedGame = false; 
        
        SaveGame(); 
    }

    // Call this for "Delete Save" buttons
    public void DeleteSaveFile()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            currentSaveData = new SavedGameData(); // Reset memory
            Debug.Log("Save file deleted.");
        }
    }
}
