
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class KeypadUiController : MonoBehaviour
{
    private static string pin = "";
    [SerializeField] TextMeshProUGUI displayTextMesh;
    private string currentPin = "";

    private KeypadController keypadController;

    [SerializeField] AudioClip buttonClick;
    [SerializeField] AudioClip incorrectPinSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    private void addPinDigit(string digit)
    {
        currentPin += digit;
        displayTextMesh.text = currentPin;
    }

    public void DigitPress(string digit)
    {
        Debug.Log("Pressed " + digit);
        playSound(buttonClick);
        if (currentPin.Length>= pin.Length)
        {
            return;
        }

        addPinDigit(digit);
    }

    private bool checkPin()
    {
        return pin.Equals(currentPin);
    }

    private void playSound(AudioClip audioClip)
    {
        audioSource.resource = audioClip;
        audioSource.Play();
    }

    public void EnterPress()
    {
        playSound(buttonClick);
        if (checkPin())
        {
            // correct pin logic
            Debug.Log("Correct Pin");
            ExitPress();
            
            keypadController.correctPin();
        }
        else
        {
            // Incorrect pin logic
            Debug.Log("Incorrect Pin");
            playSound(incorrectPinSound);
        }

        currentPin = "";
        displayTextMesh.text = currentPin;
    }

    public void ExitPress()
    {
        playSound(buttonClick);
        currentPin = "";
        gameObject.SetActive(false);
        FirstPersonController.movementDisabled = false;
        keypadController.setInteractable(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void openUi(KeypadController keypad)
    {
        keypadController = keypad;
        pin = keypadController.pin;
        gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
