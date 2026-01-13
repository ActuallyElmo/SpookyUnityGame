using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class KeypadController : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject keypadCanvas;
    private KeypadUiController keypadUiController;
    [SerializeField] public string pin;
    [SerializeField] DoorController door;
    [SerializeField] FinalDoorController finalDoorController;

    [SerializeField] AudioClip correctPinSound;
    private AudioSource audioSource;

    void Awake()
    {   
        if(keypadCanvas != null)
            keypadUiController = keypadCanvas.GetComponent<KeypadUiController>();
            
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    private void openUi()
    {
        keypadUiController.openUi(this);
        // disable movement
        FirstPersonController.movementDisabled = true;

        // disable interact
        gameObject.layer = 0;
    }

    public void Interact()
    {
        openUi();

    }

    public void setInteractable(bool interactable)
    {
        if (interactable)
        {
            gameObject.layer = 10;
        }
        else
        {
            gameObject.layer = 0;
        }
    }

    public void correctPin()
    {
        audioSource.resource = correctPinSound;
        audioSource.Play();
        setInteractable(false);

        if(finalDoorController != null)
        {
            finalDoorController.unlockKeypad();
        }
        else
        {
            door.isLocked = false;
        }
            
    }
}
