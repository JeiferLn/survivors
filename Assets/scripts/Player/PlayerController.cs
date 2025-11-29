using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ------------- MOVEMENT VARIABLES -------------
    [Header("Movimiento")]
    [SerializeField]
    private float moveSpeed = 10f;

    [SerializeField]
    private float aimingMoveSpeed = 4f;

    // ------------- GRAVITY VARIABLES -------------
    [SerializeField]
    private float gravity = -9.81f;

    private float verticalVelocity;
    private CharacterController controller;

    // ------------- CAMERA VARIABLES -------------
    [Header("Camara")]
    [SerializeField]
    private Transform cameraTransform;

    [SerializeField]
    private LayerMask walkAreaLayer;

    // ------------- INPUT VARIABLES -------------
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isAiming = false;

    // ------------- START -------------
    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // ------------- INPUT ACTIONS -------------
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    // ------------- SET AIMING -------------
    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    // ------------- UPDATE -------------
    private void Update()
    {
        MovePlayer();
        ApplyGravity();
        HandleRotation();
    }

    // ------------- MOVEMENT -------------
    private void MovePlayer()
    {
        float currentSpeed = isAiming ? aimingMoveSpeed : moveSpeed;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        Vector3 direction = forward * moveInput.y + right * moveInput.x;
        direction.Normalize();

        controller.Move(direction * currentSpeed * Time.deltaTime);
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

        controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    // ------------- ROTATION HANDLER -------------
    private void HandleRotation()
    {
        bool usingGamepad = Gamepad.current != null && lookInput.sqrMagnitude > 0.1f;

        if (usingGamepad)
        {
            RotateWithGamepad();
        }
        else
        {
            RotateTowardsMouse();
        }
    }

    // ------------- ROTATION WITH GAMEPAD -------------
    private void RotateWithGamepad()
    {
        Vector3 dir = new Vector3(lookInput.x, 0, lookInput.y);

        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    // ------------- ROTATION WITH MOUSE -------------
    private void RotateTowardsMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 300f, walkAreaLayer))
        {
            RotateTowards(hit.point);
        }
    }

    // ------------- ROTATE TOWARDS -------------
    private void RotateTowards(Vector3 worldPoint)
    {
        Vector3 lookPoint = worldPoint;
        lookPoint.y = transform.position.y;
        Vector3 dir = (lookPoint - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}
