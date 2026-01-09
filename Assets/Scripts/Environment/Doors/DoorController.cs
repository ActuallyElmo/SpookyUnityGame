using UnityEngine;
using UnityEngine.Audio;

public class DoorController : MonoBehaviour
{
    private Animator doorAnimator;

    public bool isOpen = false;

    [SerializeField] AudioClip openSound;
    [SerializeField] AudioClip closeSound;

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

    public void PlayOpenSound()
    {
        AudioManager.Instance.PlaySoundEffect(openSound, gameObject);
    }

    public void PlayCloseSound()
    {
        AudioManager.Instance.PlaySoundEffect(closeSound, gameObject);
    }
}
