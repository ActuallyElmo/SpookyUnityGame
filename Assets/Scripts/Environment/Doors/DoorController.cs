using UnityEngine;
using UnityEngine.Audio;

public class DoorController : MonoBehaviour
{
    private Animator doorAnimator;

    private bool isOpen = false;

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
            doorAnimator.Play("DoorClose");
            isOpen = false;
        }
        else
        {
            doorAnimator.Play("DoorOpen");
            isOpen = true;
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
