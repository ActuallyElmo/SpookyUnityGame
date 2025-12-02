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
    [SerializeField] private string pickup = "Pickup";          
    [SerializeField] private string drop = "Drop";    

    private InputAction movementAction;
    private InputAction rotationAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private InputAction pickupAction;
    private InputAction dropAction;


    // Public properties accessed by controller scripts
    public Vector2 MovementInput { get; private set; }           // Stores WASD input
    public Vector2 RotationInput { get; private set; }           // Stores mouse input
    public bool JumpTriggered { get; private set; }              // True while jump is pressed
    public bool SprintTriggered { get; private set; }            // True while sprint is pressed
    public bool CrouchTriggered { get; private set; }            // True while crouch is pressed
    public bool PickupTriggered { get; private set; }           
    public bool DropTriggered { get; private set; }             


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
        pickupAction = mapReference.FindAction(pickup);
        dropAction = mapReference.FindAction(drop);

        // Initialize triggers
        PickupTriggered = false;
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

        pickupAction.performed += ctx => PickupTriggered = true;
        pickupAction.canceled += ctx => PickupTriggered = false;

        dropAction.performed += ctx => DropTriggered = true;
        dropAction.canceled += ctx => DropTriggered = false;
    }

    private void OnEnable()
    {
        playerControls.FindActionMap(actionMapName).Enable();
        pickupAction.Enable();
        dropAction.Enable();

    }

    private void OnDisable()
    {
        playerControls.FindActionMap(actionMapName).Disable();
        pickupAction.Disable();
        dropAction.Disable();

    }

        public void ResetPickupTrigger()
    {
        PickupTriggered = false;
    }
        public void ResetDropTrigger()
    {
        DropTriggered = false;
    }


}
