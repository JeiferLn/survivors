using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField]
    private float moveSpeed = 6f;

    [SerializeField]
    private float gravity = -9.81f;

    private float verticalVelocity;
    private CharacterController controller;

    [Header("Camara")]
    [SerializeField]
    private float lookSensitivity = 2f;

    [SerializeField]
    private Transform cameraTransform;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float cameraPitch = 0f;

    // ------------- START -------------
    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // ------------- INPUT SYSTEM -------------
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    // ------------- UPDATE -------------
    private void Update()
    {
        MovePlayer();
        ApplyGravity();
        RotateTowardsMouse();
        RotateCamera();
    }

    // ------------- MOVEMENT -------------
    private void MovePlayer()
    {
        Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y);

        direction = cameraTransform.TransformDirection(direction);
        direction.y = 0f;
        direction.Normalize();

        controller.Move(direction * moveSpeed * Time.deltaTime);
    }

    // ------------- GRAVITY -------------
    private void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 verticalMove = new Vector3(0, verticalVelocity, 0);
        controller.Move(verticalMove * Time.deltaTime);
    }

    // ------------- ROTATION -------------
    private void RotateTowardsMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            Vector3 point = hit.point;
            point.y = transform.position.y;

            Vector3 dir = (point - transform.position).normalized;

            if (dir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    // ------------- CAMERA ROTATION -------------
    private void RotateCamera()
    {
        float mouseY = lookInput.y * lookSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -70f, 70f);

        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }
}
