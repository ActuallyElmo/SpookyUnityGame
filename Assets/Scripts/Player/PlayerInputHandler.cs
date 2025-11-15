using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;   

    [Header("Action Map Name Reference")]
    [SerializeField] private string actionMapName = "Player";     // Action map used for player controls

    [Header("Action Name References")]
    [SerializeField] private string movement = "Move";
    [SerializeField] private string rotation = "Look";           
    [SerializeField] private string jump = "Jump";               
    [SerializeField] private string sprint = "Sprint";           
    [SerializeField] private string crouch = "Crouch";           

    private InputAction movementAction;
    private InputAction rotationAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    // Public properties accessed by controller scripts
    public Vector2 MovementInput { get; private set; }           // Stores WASD input
    public Vector2 RotationInput { get; private set; }           // Stores mouse input
    public bool JumpTriggered { get; private set; }              // True while jump is pressed
    public bool SprintTriggered { get; private set; }            // True while sprint is pressed
    public bool CrouchTriggered { get; private set; }            // True while crouch is pressed

    private void Awake()
    {
        // Finds the action map from the InputActionAsset
        InputActionMap mapReference = playerControls.FindActionMap(actionMapName);

        // Assigns each action based on its name
        movementAction = mapReference.FindAction(movement);
        rotationAction = mapReference.FindAction(rotation);
        jumpAction = mapReference.FindAction(jump);
        sprintAction = mapReference.FindAction(sprint);
        crouchAction = mapReference.FindAction(crouch);

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
    }

    private void OnEnable()
    {
        // Enables all actions in the selected action map
        playerControls.FindActionMap(actionMapName).Enable();
    }

    private void OnDisable()
    {
        // Disables all actions in the selected action map
        playerControls.FindActionMap(actionMapName).Disable();
    }
}
