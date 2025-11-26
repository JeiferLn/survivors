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

    // ------------- INPUT VARIABLES -------------
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isAiming = false;

    // ------------- LINE RENDERER -------------
    [Header("Laser")]
    private LineRenderer lineRenderer;
    [SerializeField]
    private float laserDistance = 10f;

    [SerializeField]
    private Transform laserOrigin;

    // ------------- START -------------
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
            lineRenderer.enabled = false;
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

    public void OnAim(InputAction.CallbackContext ctx)
    {
        isAiming = ctx.ReadValueAsButton();
    }

    // ------------- UPDATE -------------
    private void Update()
    {
        MovePlayer();
        ApplyGravity();
        HandleRotation();
        HandleLaser();
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

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            Vector3 lookPoint = hit.point;
            lookPoint.y = transform.position.y;

            Vector3 dir = (lookPoint - transform.position);
            dir.y = 0;

            if (dir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    // ------------- LASER SYSTEM -------------
    private void HandleLaser()
    {
        if (lineRenderer == null)
            return;

        if (!isAiming)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;

        Vector3 start = laserOrigin != null
            ? laserOrigin.position
            : transform.position + transform.forward * 0.5f + Vector3.up * 1.0f;

        Vector3 dir = transform.forward;
        Vector3 end = start + dir * laserDistance;

        if (Physics.Raycast(start, dir, out RaycastHit hit, laserDistance))
        {
            end = hit.point;
        }

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
}
