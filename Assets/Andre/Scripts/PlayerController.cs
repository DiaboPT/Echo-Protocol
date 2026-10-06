using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference crouchAction;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 89f;

    [Header("Crouching")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float crouchSpeed = 3f;

    private CharacterController controller;

    private float verticalVelocity;
    private float cameraPitch;
    private float currentSpeed;

    private bool isCrouching;

    private void Awake() {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void OnEnable() {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
        crouchAction.action.Enable();
    }

    private void OnDisable() {
        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
        sprintAction.action.Disable();
        crouchAction.action.Disable();
    }

    private void Start() {
        LockCursor();
    }

    private void Update() {
        HandleLook();
        HandleMovement();
        HandleCrouching();
    }

    private void HandleLook() {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -maxLookAngle,
            maxLookAngle
        );

        playerCamera.transform.localRotation =
        Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleMovement() {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

        Vector3 inputDirection =
        (transform.right * moveInput.x +
        transform.forward * moveInput.y).normalized;

        bool sprinting =
        sprintAction.action.IsPressed() &&
        moveInput.y > 0f &&
        !isCrouching;

        float targetSpeed = isCrouching
        ? crouchSpeed
        : sprinting ? sprintSpeed : walkSpeed;

        float speedChange = inputDirection.magnitude > 0f
        ? acceleration
        : deceleration;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed * inputDirection.magnitude,
            speedChange * Time.deltaTime
        );

        Vector3 horizontalVelocity =
        inputDirection * currentSpeed;

        if (controller.isGrounded) {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (jumpAction.action.WasPressedThisFrame() &&
                !isCrouching
            ) {
                verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity =
        horizontalVelocity +
        Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCrouching() {
        if (crouchAction.action.WasPressedThisFrame()) {
            isCrouching = true;
            controller.height = crouchingHeight;
        }

        if (crouchAction.action.WasReleasedThisFrame()) {
            isCrouching = false;
            controller.height = standingHeight;
        }
    }

    private void LockCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnApplicationFocus(bool hasFocus) {
        if (hasFocus)
            LockCursor();
    }
}
