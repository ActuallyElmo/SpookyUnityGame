using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator doorAnimator;

    private const string doubleDoorTag = "DoubleDoor";

    private bool isOpen = false;

    private void Awake()
    {
        if(transform.parent != null && transform.parent.CompareTag(doubleDoorTag))
        {
            doorAnimator = transform.parent.GetComponent<Animator>();
            return;
        }
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
