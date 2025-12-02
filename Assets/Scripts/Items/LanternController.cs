using UnityEngine;

public class LanternController : MonoBehaviour
{
    public Light lanternLight; 
    public PlayerInputHandler inputHandler; 

    void Update()
    {
        if (inputHandler == null || lanternLight == null) return;

        
        if (inputHandler.PickupTriggered)
        {
            lanternLight.enabled = !lanternLight.enabled;
            inputHandler.ResetPickupTrigger(); 
        }
    }
}
