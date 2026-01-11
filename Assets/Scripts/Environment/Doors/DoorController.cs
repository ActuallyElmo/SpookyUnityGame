using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class DoorController : MonoBehaviour
{
    private Animator doorAnimator;

    public bool isOpen = false;

    [SerializeField] AudioClip openSound;
    [SerializeField] AudioClip closeSound;

    [SerializeField] public GameObject neededKey = null;
    [SerializeField] public bool isLocked = false;

    private void Awake()
    {
        doorAnimator = GetComponent<Animator>();
    }

    private void PlayAnimation()
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

    private IEnumerator DisplayLockedMessage()
    {
        const int displaySeconds = 2;

        HudController.Instance.EnableInteractionText("You need a key for this door", "");
        yield return new WaitForSeconds(displaySeconds);
        HudController.Instance.DisableInteractionText();
    }

    public void Interact()
    {
        if(neededKey != null)
        {
            PickupItem pickupItemKey = neededKey.GetComponent<PickupItem>();
            if (isLocked && !pickupItemKey.isHeld)
            {
                StartCoroutine(DisplayLockedMessage());
                return;
            }
            
        }

        isLocked = false;

        PlayAnimation();
    }

    public void PlayOpenSound()
    {
        AudioManager.Instance.PlaySoundEffect(openSound, gameObject);
    }

    public void PlayCloseSound()
    {
        AudioManager.Instance.PlaySoundEffect(closeSound, gameObject);
    }
}
