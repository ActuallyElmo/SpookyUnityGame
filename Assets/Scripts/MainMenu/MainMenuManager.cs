using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Animator fadeElementAnim;

    public Button continueGameButton;

    public GameObject playerSettingsTab;

    void Start()
    {
        fadeElementAnim.SetTrigger("FadeOut");

        if(GameSaveManager.instance.VerifyForSavedGame())
        {
            continueGameButton.interactable = true;
        }
        else
        {
            continueGameButton.interactable = false;
        }
    }

    public void StartNewGame()
    {
        GameSaveManager.instance.StartNewGame();
        StartCoroutine(LoadGameScene());
    }

    public void LoadSavedGame()
    {
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        fadeElementAnim.SetTrigger("FadeIn");
        yield return new WaitForSeconds(2f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Destroy(this.gameObject);
    }

    public void TogglePlayerSettings()
    {
        playerSettingsTab.SetActive(!playerSettingsTab.activeSelf);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
