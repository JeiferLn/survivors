using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;

    [Header("Movimiento")]
    [SerializeField]
    private float moveSpeed = 6f;

    [SerializeField]
    private float gravity = -9.81f;
    private float verticalVelocity;

    [Header("Rotación")]
    [SerializeField]
    private Transform cameraTransform;

    [SerializeField]
    private float lookSensitivity = 1f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float cameraPitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // --- INPUT EVENTS ---
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    // --------------------

    void Update()
    {
        Movement();
        Rotation();
    }

    void Movement()
    {
        Vector3 input = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        Vector3 moveDir = Vector3.zero;

        if (input.magnitude >= 0.1f)
        {
            moveDir = cameraTransform.forward * input.z + cameraTransform.right * input.x;
            moveDir.y = 0f;
            moveDir.Normalize();
        }

        Vector3 horizontalMovement = moveDir * moveSpeed;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 finalMovement = horizontalMovement + new Vector3(0, verticalVelocity, 0);

        controller.Move(finalMovement * Time.deltaTime);
    }

    void Rotation()
    {
        transform.Rotate(Vector3.up, lookInput.x * lookSensitivity);

        cameraPitch -= lookInput.y * lookSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}
