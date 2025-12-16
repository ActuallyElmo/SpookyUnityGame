using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator doorAnimator;

    private bool isOpen = false;

    private void Awake()
    {
        doorAnimator = GetComponent<Animator>();
    }

    public void PlayAnimation()
    {
        if(isOpen)
        {
            doorAnimator.Play("DoorClose");
            isOpen = false;
        }
        else
        {
            doorAnimator.Play("DoorOpen");
            isOpen = true;
        }

    }
}
