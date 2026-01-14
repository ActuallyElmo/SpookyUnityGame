using System;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed;              // Base walking speed
    [SerializeField] private float sprintMultiplier;       // Speed multiplier when sprinting
    [SerializeField] private float crouchMultiplier;       // Speed multiplier when crouched

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce;              // Initial jump force
    [SerializeField] private float gravityMultiplier;      // Extra gravity applied while falling
    [SerializeField] private bool disableJump = false;

    [Header("Crouch Parameters")]
    [SerializeField] private float defaultHeight;          // Controller height while standing
    [SerializeField] private float crouchHeight;           // Controller height while crouching
    [SerializeField] private float crouchSmoothTime;       // Reserved for smooth transitions

    [Header("Look Parameters")]
    [SerializeField] private float mouseSensitivity;       // Mouse sensitivity for rotation
    [SerializeField] private float upDownLookRange;        // Clamp for vertical camera rotation

    [Header("References")]
    [SerializeField] private CharacterController characterController;  // Main movement component
    [SerializeField] private Camera mainCamera;                        // Player camera
    [SerializeField] private PlayerInputHandler playerInputHandler;    // Custom input wrapper

    public Vector3 currentMovement;           // Stores movement values
    private float verticalRotation;            // Tracks camera pitch
    private float targetHeight;                
    private float heightVelocity;              // Reserved for smooth transitions
    private bool isCrouching = false;          // Crouch state

    public static bool movementDisabled = false;

    // Computes final movement speed based on sprint or crouch
    private float currentSpeed =>
        walkSpeed * (playerInputHandler.SprintTriggered && !isCrouching ? sprintMultiplier :
        (isCrouching ? crouchMultiplier : 1));

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        targetHeight = defaultHeight;               
    }

    void Update()
    {
        if (movementDisabled)
        {
            return;
        }
        if(playerInputHandler.MenuTriggered)
        {
            GameSceneManager.instance.ToggleMenu();
            playerInputHandler.ResetMenuTrigger();
        }
        HandleCrouching();
        HandleMovement();
        HandleRotation();
        HandleJumping();
    }

    private Vector3 CalculateWorldDirection()
    {
        // Converts input (WASD) to world-space direction based on player orientation
        Vector3 inputDirection = new Vector3(playerInputHandler.MovementInput.x, 0f, playerInputHandler.MovementInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection.normalized;
    }

    private void HandleJumping()
    {
        if (characterController.isGrounded)
        {
            currentMovement.y = -0.5f;  // Keeps controller grounded

            if (playerInputHandler.JumpTriggered && disableJump == false)
            {
                currentMovement.y = jumpForce;  // Apply jump
            }
        }
        else
        {
            // Apply extra gravity for better fall feel
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    private void HandleCrouching()
    {
        // Toggles crouch height based on input and ability to stand
        if (playerInputHandler.CrouchTriggered)
        {
            targetHeight = crouchHeight;
            isCrouching = true;
        }
        else
        {
            if (!CanStandUp())
            {
                targetHeight = crouchHeight;
                isCrouching = true;
            }
            else
            {
                targetHeight = defaultHeight;
                isCrouching = false;
            }
        }

        // Smoothly interpolate the controller height
        characterController.height = Mathf.SmoothDamp(
            characterController.height,
            targetHeight,
            ref heightVelocity,
            crouchSmoothTime
        );
    }

    private bool CanStandUp()
    {
        float radius = characterController.radius;
        float standHeight = defaultHeight;

        // Compute bottom and top of the capsule if standing
        Vector3 bottom = transform.position + characterController.center - Vector3.up * (characterController.height / 2 - radius);
        Vector3 top = bottom + Vector3.up * (standHeight - 2 * radius);

        // Check for any colliders in the space above using OverlapCapsule
        Collider[] hits = Physics.OverlapCapsule(bottom, top, radius, LayerMask.NameToLayer("Map"), QueryTriggerInteraction.Ignore);
        foreach (var hit in hits)
        {
            // Ignore the player's own collider
            if (hit != characterController)
                return false; // Something is blocking the space above
        }

        return true; // Nothing blocking, safe to stand
    }

    private void HandleMovement()
    {
        // Applies horizontal movement based on input and speed
        Vector3 worldDirection = CalculateWorldDirection();
        currentMovement.x = worldDirection.x * currentSpeed;
        currentMovement.z = worldDirection.z * currentSpeed;

        characterController.Move(currentMovement * Time.deltaTime);
    }

    private void ApplyHorizontalRotation(float rotationAmount)
    {
        // Rotates the player body around the Y-axis
        transform.Rotate(0, rotationAmount, 0);
    }

    private void ApplyVerticalRotation(float rotationAmount)
    {
        // Adjusts camera pitch and clamps it
        verticalRotation = Mathf.Clamp(verticalRotation - rotationAmount, -upDownLookRange, upDownLookRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleRotation()
    {
        // Mouse input converted to rotation
        float mouseXRotation = playerInputHandler.RotationInput.x * mouseSensitivity;
        float mouseYRotation = playerInputHandler.RotationInput.y * mouseSensitivity;

        ApplyHorizontalRotation(mouseXRotation);
        ApplyVerticalRotation(mouseYRotation);
    }
}
