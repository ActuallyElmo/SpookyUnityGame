using System.Collections;
using Unity.Mathematics;
using UnityEditor.Overlays;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameSceneManager : MonoBehaviour
{
    public Animator fadeElemAnim;
    public static GameSceneManager instance;

    public GameObject enemy;
    public GameObject flashLight;
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
        yield return new WaitForSecondsRealtime(0.5f);
        yield return new WaitForEndOfFrame();

        if(GameSaveManager.instance == null)
        {
            Debug.LogError("No Game Save Manager Found, game can't be saved or loaded");
            fadeElemAnim.SetTrigger("FadeOut");
            yield break;
        }

        SavedGameData saveData = GameSaveManager.instance.currentSaveData;

        if(!saveData.isNewSave)
        {
            Debug.Log("Loading saved game");

            PlayerSingleton.instance.transform.position = saveData.playerPosition;
            PlayerSingleton.instance.transform.rotation = Quaternion.Euler(saveData.playerRotation);

            //Restore item positions
            flashLight.transform.position = GameSaveManager.instance.currentSaveData.flashlightPosition;
            if(flashLight.transform.position.x == -1 && flashLight.transform.position.y == -1) 
            {
                yield return new WaitForEndOfFrame();
                PlayerSingleton.instance.GetComponent<PlayerPickupSystem>().PickUpObject(flashLight);
            }

            //Restore world state
            yield return new WaitForEndOfFrame();
            LoadDoorStates(saveData);
        }
        else
        {
            saveData.isNewSave = false;
            SaveGame(); 
        }

        yield return new WaitForSeconds(0.5f);
        fadeElemAnim.SetTrigger("FadeOut");

        StartCoroutine(AutomaticallySaveGame());
    }

    private void LoadDoorStates(SavedGameData data)
    {
        if (data.mapDoorStates == null || data.mapDoorStates.Length == 0) return;

        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        Vector3 playerPos = PlayerSingleton.instance.transform.position;
        List<GameObject> sortedDoors = doors.OrderBy(d => Vector3.Distance(playerPos, d.transform.position)).ToList();

        for (int i = 0; i < sortedDoors.Count; i++)
        {
            if (i >= data.mapDoorStates.Length) break;

            var doorScript = sortedDoors[i].GetComponent<DoorController>(); 
            
            if(doorScript != null)
            {
                bool shouldBeOpen = (data.mapDoorStates[i] == 1);
                doorScript.isOpen = shouldBeOpen; 
                doorScript.UpdateDoorState();
            }
        }
    }

    IEnumerator AutomaticallySaveGame()
    {
        yield return new WaitForSeconds(30f);
        
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
        int[] nearbyDoors = new int[50];
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        Vector3 playerPos = PlayerSingleton.instance.transform.position;
        List<GameObject> sortedDoors = doors.OrderBy(d => Vector3.Distance(playerPos, d.transform.position)).ToList();

        GameSaveManager.instance.currentSaveData.mapDoorStates = new int[sortedDoors.Count];

        for (int i = 0; i < sortedDoors.Count; i++)
        {
            var doorScript = sortedDoors[i].GetComponent<DoorController>(); 
            
            // 1 if open, 0 if closed
            if(doorScript != null)
            {
                GameSaveManager.instance.currentSaveData.mapDoorStates[i] = doorScript.isOpen ? 1 : 0; 
            }
        }


        GameSaveManager.instance.SaveGame();
    }

}
