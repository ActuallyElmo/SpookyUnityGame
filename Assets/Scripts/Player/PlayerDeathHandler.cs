using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    public bool playerIsDead = false;
    public GameObject deathCameraParent;
    public void OnDeath()
    {
        if(playerIsDead) return;
        playerIsDead = true;
        deathCameraParent.SetActive(true);
        GetComponent<FirstPersonController>().enabled = false;
        GetComponent<PlayerSoundManager>().enabled = false;
        GameSceneManager.instance.OnDeath();
    }
}
