using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator doorAnimator;

    public bool isOpen = false;

    private void Awake()
    {
        doorAnimator = GetComponent<Animator>();
    }

    public void PlayAnimation()
    {
        if(isOpen)
        {
            isOpen = false;
        }
        else
        {
            isOpen = true;
        }

        UpdateDoorState();

    }

    public void UpdateDoorState()
    {
        if(doorAnimator == null)
            return;
            
        if(isOpen)
        {
            doorAnimator.SetTrigger("Open");
        }
        else
        {
            doorAnimator.SetTrigger("Close");
        }
    }
}
