using UnityEngine;
using UnityEngine.InputSystem;

// Movimento FPS: WASD andar, Shift correr, Espaço saltar, rato olhar, Esc solta o rato.
// Pôr na Capsule. A Main Camera fica como filha da Capsule.
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.5f;   // salto normal (o reforçado está no SuitAbilities)
    public float gravity = -20f;

    [Header("Câmara")]
    public Transform cameraTransform; // arrastar a Main Camera
    public float mouseSensitivity = 0.1f;

    CharacterController controller;
    SuitAbilities abilities;
    float verticalVelocity;
    float pitch;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        abilities = GetComponent<SuitAbilities>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        // --- Olhar ---
        Vector2 look = mouse.delta.ReadValue() * mouseSensitivity;
        transform.Rotate(0f, look.x, 0f);                 // o corpo roda para os lados
        pitch = Mathf.Clamp(pitch - look.y, -85f, 85f);   // a câmara sobe/desce, com limite
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // --- Andar ---
        Vector3 move = Vector3.zero;
        if (kb.wKey.isPressed) move += transform.forward;
        if (kb.sKey.isPressed) move -= transform.forward;
        if (kb.dKey.isPressed) move += transform.right;
        if (kb.aKey.isPressed) move -= transform.right;
        move = Vector3.ClampMagnitude(move, 1f);          // na diagonal não anda mais rápido

        float speed = walkSpeed;
        if (kb.leftShiftKey.isPressed) speed = runSpeed;

        // --- Saltar e gravidade ---
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;   // mantém colado ao chão
            if (kb.spaceKey.wasPressedThisFrame)
                verticalVelocity = Mathf.Sqrt(GetJumpHeight() * -2f * gravity);
        }
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        // --- Esc solta o rato (útil no editor) ---
        if (kb.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Altura do salto: reforçada se o Salto do Fato estiver ligado
    public float GetJumpHeight()
    {
        if (abilities != null && abilities.jumpActive)
            return abilities.boostedJumpHeight;
        return jumpHeight;
    }
}
