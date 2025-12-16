using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions playerControls;


    private InputAction movementAction;
    private InputAction rotationAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private InputAction interactAction;
    private InputAction dropAction;


    // Public properties accessed by controller scripts
    public Vector2 MovementInput { get; private set; }           // Stores WASD input
    public Vector2 RotationInput { get; private set; }           // Stores mouse input
    public bool JumpTriggered { get; private set; }              // True while jump is pressed
    public bool SprintTriggered { get; private set; }            // True while sprint is pressed
    public bool CrouchTriggered { get; private set; }            // True while crouch is pressed
    public bool InteractTriggered { get; private set; }           
    public bool DropTriggered { get; private set; }             


    private void Awake()
    {
        playerControls = new PlayerInputActions();
        playerControls.Enable();

        // Assigns each action based on its name
        movementAction = playerControls.Player.Move;
        rotationAction = playerControls.Player.Look;
        jumpAction = playerControls.Player.Jump;
        sprintAction = playerControls.Player.Sprint;
        crouchAction = playerControls.Player.Crouch;
        interactAction = playerControls.Player.Interact;
        dropAction = playerControls.Player.Drop;

        // Initialize triggers
        InteractTriggered = false;
        DropTriggered = false;

        // Connects input events to callbacks
        SubscribeActionValuesToInputEvents();

    }

    private void SubscribeActionValuesToInputEvents()
    {
        movementAction.performed += inputInfo => MovementInput = inputInfo.ReadValue<Vector2>();
        movementAction.canceled += inputInfo => MovementInput = Vector2.zero;

        rotationAction.performed += inputInfo => RotationInput = inputInfo.ReadValue<Vector2>();
        rotationAction.canceled += inputInfo => RotationInput = Vector2.zero;

        jumpAction.performed += inputInfo => JumpTriggered = true;
        jumpAction.canceled += inputInfo => JumpTriggered = false;

        sprintAction.performed += inputInfo => SprintTriggered = true;
        sprintAction.canceled += inputInfo => SprintTriggered = false;

        crouchAction.performed += inputInfo => CrouchTriggered = true;
        crouchAction.canceled += inputInfo => CrouchTriggered = false;

        interactAction.performed += ctx => InteractTriggered = true;
        interactAction.canceled += ctx => InteractTriggered = false;

        dropAction.performed += ctx => DropTriggered = true;
        dropAction.canceled += ctx => DropTriggered = false;
    }

    private void OnEnable()
    {
        //playerControls.FindActionMap(actionMapName).Enable();
        interactAction.Enable();
        dropAction.Enable();

    }

    private void OnDisable()
    {
        //playerControls.FindActionMap(actionMapName).Disable();
        interactAction.Disable();
        dropAction.Disable();

    }

    public void ResetPickupTrigger()
    {
        InteractTriggered = false;
    }
    public void ResetDropTrigger()
    {
        DropTriggered = false;
    }


}
