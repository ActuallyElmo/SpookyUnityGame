using System.Collections;
using Unity.Mathematics;
using UnityEditor.Overlays;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public GameObject deathScreen;
    public Animator fadeElemAnim;
    public static GameSceneManager instance;

    public GameObject enemy;
    public GameObject flashLight;

    public bool gameIsLoaded = true;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(SetUpScene());
    }

    IEnumerator SetUpScene()
    {
        gameIsLoaded = false;
        fadeElemAnim.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(0.5f);
        yield return new WaitForEndOfFrame();

        if(GameSaveManager.instance == null)
        {
            Debug.LogError("No Game Save Manager Found, game can't be saved or loaded");
            fadeElemAnim.SetTrigger("FadeOut");
            gameIsLoaded = true;
            yield break;
        }

        SavedGameData saveData = GameSaveManager.instance.currentSaveData;

        if(!saveData.isNewSave)
        {
            Debug.Log("Loading saved game");

            CharacterController controller = PlayerSingleton.instance.GetComponent<CharacterController>();

            if (controller != null) controller.enabled = false;

            PlayerSingleton.instance.transform.position = saveData.playerPosition;
            PlayerSingleton.instance.transform.rotation = Quaternion.Euler(saveData.playerRotation);

            if (controller != null) controller.enabled = true;

            //Restore item positions
            flashLight.transform.position = GameSaveManager.instance.currentSaveData.flashlightPosition;
            if(flashLight.transform.position.x < -0.9f && flashLight.transform.position.x > -1.1f && flashLight.transform.position.y > -1.1f && flashLight.transform.position.y < -0.9f) 
            {
                yield return new WaitForEndOfFrame();
                PlayerSingleton.instance.GetComponent<PlayerPickupSystem>().PickUpObject(flashLight);
            }

            //Restore world state
            yield return new WaitForEndOfFrame();
            LoadObjectStates("Door", saveData.mapDoorStates);
            LoadObjectStates("Locker", saveData.mapLockerStates);

            enemy.GetComponent<Rigidbody>().isKinematic = true;
            yield return new WaitForEndOfFrame();
            enemy.transform.position = saveData.enemyPosition;
            enemy.transform.rotation = Quaternion.Euler(saveData.enemyRotation);
            yield return new WaitForEndOfFrame();
            enemy.GetComponent<Rigidbody>().isKinematic = false;

            Debug.Log("Loaded saved game");
        }
        else
        {
            saveData.isNewSave = false;
            SaveGame(); 
        }

        yield return new WaitForSeconds(0.5f);
        fadeElemAnim.SetTrigger("FadeOut");
        yield return new WaitForSeconds(1.5f);
        gameIsLoaded = true;

        StartCoroutine(AutomaticallySaveGame());
    }

    private void LoadObjectStates(string tag, int[] states)
    {
        if (states == null || states.Length == 0) return;

        List<GameObject> sortedObjects = GetSortedObjects(tag);

        for (int i = 0; i < sortedObjects.Count; i++)
        {
            if (i >= states.Length) break;

            var doorScript = sortedObjects[i].GetComponent<DoorController>(); 
            
            if(doorScript != null)
            {
                bool shouldBeOpen = (states[i] == 1);
                doorScript.isOpen = shouldBeOpen; 
                doorScript.UpdateDoorState();
            }
        }
    }

    IEnumerator AutomaticallySaveGame()
    {
        yield return new WaitForSeconds(30f);
        
        if(PlayerSingleton.instance.GetComponent<PlayerDeathHandler>().playerIsDead)
            yield break;
        
        SaveGame();
        StartCoroutine(AutomaticallySaveGame());
    }

    public void SaveGame()
    {
        GameSaveManager.instance.currentSaveData.isNewSave = false; 
        GameSaveManager.instance.currentSaveData.hasSavedGame = true; 
        GameSaveManager.instance.currentSaveData.playerPosition = PlayerSingleton.instance.transform.position;
        GameSaveManager.instance.currentSaveData.playerRotation = PlayerSingleton.instance.transform.rotation.eulerAngles;

        Vector3 oldPosition = Vector3.zero;
        if(PlayerSingleton.instance.GetComponent<PlayerPickupSystem>().heldObject != null)
        {
            oldPosition = PlayerSingleton.instance.GetComponent<PlayerPickupSystem>().heldObject.transform.position;
            PlayerSingleton.instance.GetComponent<PlayerPickupSystem>().heldObject.transform.position = new Vector3(-1, -1); //easy to recall what item was held by the player
        }
        //Item positions
        GameSaveManager.instance.currentSaveData.flashlightPosition = flashLight.transform.position;

        if(PlayerSingleton.instance.GetComponent<PlayerPickupSystem>().heldObject != null)
        {
            PlayerSingleton.instance.GetComponent<PlayerPickupSystem>().heldObject.transform.position = oldPosition;
        }

        //World state
        GameSaveManager.instance.currentSaveData.mapDoorStates = GetStatesForTag("Door");
        GameSaveManager.instance.currentSaveData.mapLockerStates = GetStatesForTag("Locker");

        GameSaveManager.instance.currentSaveData.enemyPosition = enemy.transform.position;
        GameSaveManager.instance.currentSaveData.enemyRotation = enemy.transform.rotation.eulerAngles;

        GameSaveManager.instance.SaveGame();
    }

    private int[] GetStatesForTag(string tag)
    {
        List<GameObject> sortedObjects = GetSortedObjects(tag);
        int[] states = new int[sortedObjects.Count];

        for (int i = 0; i < sortedObjects.Count; i++)
        {
            var doorScript = sortedObjects[i].GetComponent<DoorController>();
            if(doorScript != null)
            {
                states[i] = doorScript.isOpen ? 1 : 0;
            }
        }
        return states;
    }

    private List<GameObject> GetSortedObjects(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        
        Vector3 playerPos = PlayerSingleton.instance.transform.position;
        return objects.OrderBy(d => Vector3.Distance(playerPos, d.transform.position)).ToList();
    }

    public void OnDeath()
    {
        StartCoroutine(DeathAnimation());
    }
    
    IEnumerator DeathAnimation()
    {
        yield return new WaitForSecondsRealtime(2f);
        fadeElemAnim.gameObject.SetActive(true);
        fadeElemAnim.SetTrigger("FadeIn");
        deathScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadLastCheckpoint()
    {
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Destroy(this.gameObject);
    }

    public void ReturnToMainMenu()
    {
        if(GameSaveManager.instance != null)
            GameSaveManager.instance.DeleteSaveFile();
        
        StartCoroutine(LoadMainMenu());
    }

    IEnumerator LoadMainMenu()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenuScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Destroy(this.gameObject);
    }
}