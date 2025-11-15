using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float crouchMultiplier;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravityMultiplier;

    [Header("Crouch Parameters")] 
    [SerializeField] private float defaultHeight = 2.0f; 
    [SerializeField] private float crouchHeight = 1.0f; 
    [SerializeField] private float crouchSmoothTime = 0.1f;

    [Header("Look Parameters")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float upDownLookRange;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInputHandler playerInputHandler;

    private Vector3 currentMovement;
    private float verticalRotation;
    private float targetHeight;
    private float heightVelocity;
    private bool isCrouching = false;

    private float currentSpeed => walkSpeed * (playerInputHandler.SprintTriggered && !isCrouching ? sprintMultiplier : (isCrouching ? crouchMultiplier : 1));

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        targetHeight = defaultHeight;
    }

    void Update()
    {
        HandleCrouching();
        HandleMovement();
        HandleRotation();
        HandleJumping();
    }

    private Vector3 CalculateWorldDirection()
    {
        Vector3 inputDirection = new Vector3(playerInputHandler.MovementInput.x, 0f, playerInputHandler.MovementInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection.normalized;
    }

    private void HandleJumping()
    {
        if (characterController.isGrounded)
        {
            currentMovement.y = -0.5f;


            if (playerInputHandler.JumpTriggered && !isCrouching)
            {
                currentMovement.y = jumpForce;
            }
        }
        else
        {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    private void HandleCrouching() 
    {
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

        float newHeight = targetHeight;
        characterController.height = newHeight;
    }

    private bool CanStandUp()
    {
        return true; 
    }

    private void HandleMovement()
    {
        Vector3 worldDirection = CalculateWorldDirection();
        currentMovement.x = worldDirection.x * currentSpeed;
        currentMovement.z = worldDirection.z * currentSpeed;
        characterController.Move(currentMovement * Time.deltaTime);
    }

    private void ApplyHorizontalRotation(float rotationAmount)
    {
        transform.Rotate(0, rotationAmount, 0);
    }

    private void ApplyVerticalRotation(float rotationAmount)
    {
        verticalRotation = Mathf.Clamp(verticalRotation - rotationAmount, -upDownLookRange, upDownLookRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleRotation()
    {
        float mouseXRotation = playerInputHandler.RotationInput.x * mouseSensitivity;
        float mouseYRotation = playerInputHandler.RotationInput.y * mouseSensitivity;

        ApplyHorizontalRotation(mouseXRotation);
        ApplyVerticalRotation(mouseYRotation);
    }

}