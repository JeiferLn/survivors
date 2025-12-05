using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ------------- REFERENCES -------------
    private CharacterController controller;
    private Camera mainCam;
    private Animator animator;

    // ------------- MOVEMENT VARIABLES -------------
    [Header("Movimiento")]

    [SerializeField] private float walkMovementSpeed = 6f;
    [SerializeField] private float runMovementSpeed = 10f;
    [SerializeField] private float aimingMoveSpeed = 4f;

    public bool canMove = true;

    // ------------- GRAVITY VARIABLES -------------
    [SerializeField] private float gravity = -9.81f;
    private float verticalVelocity;

    // ------------- CAMERA VARIABLES -------------
    [Header("Camara")]
    [SerializeField] private Transform cameraTransform;

    // ------------- LAYER MASK -------------
    [Header("Layer")]
    [SerializeField] private LayerMask walkAreaLayer;

    // ------------- INPUT VARIABLES -------------
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isAiming;
    private bool isRunning;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main;
        animator = GetComponentInChildren<Animator>();
    }

    // ---------------- INPUTS --------------------
    public void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    public void OnRun(InputAction.CallbackContext ctx) => isRunning = ctx.ReadValueAsButton();
    public void OnLook(InputAction.CallbackContext ctx) => lookInput = ctx.ReadValue<Vector2>();
    public void SetAiming(bool aiming) => isAiming = aiming;

    private void Update()
    {
        if (!canMove) return;

        MovePlayer();
        ApplyGravity();
        HandleRotation();
    }

    // ---------------- MOVEMENT --------------------
    private void MovePlayer()
    {
        float animSpeed = isRunning ? 1f : 0f;

        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);

        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        float dot = Vector3.Dot(transform.forward, direction);
        bool isGoingBack = dot < -0.5f;

        float currentSpeed;

        if (isAiming)
        {
            currentSpeed = isGoingBack ? aimingMoveSpeed / 2f : aimingMoveSpeed;
        }
        else
        {
            if (isGoingBack)
            {
                currentSpeed = walkMovementSpeed / 2f;
            }
            else
            {
                currentSpeed = isRunning ? runMovementSpeed : walkMovementSpeed;
            }
        }

        Vector3 localDir = transform.InverseTransformDirection(direction);

        animator.SetFloat("Speed", animSpeed, 0.15f, Time.deltaTime);
        animator.SetFloat("Horizontal", localDir.x, 0.15f, Time.deltaTime);
        animator.SetFloat("Vertical", localDir.z, 0.15f, Time.deltaTime);

        controller.Move(direction * currentSpeed * Time.deltaTime);
    }

    // ---------------- GRAVITY --------------------
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

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    // ---------------- ROTATION HANDLER --------------------
    private void HandleRotation()
    {
        bool usingGamepad = Gamepad.current != null && lookInput.sqrMagnitude > 0.1f;

        if (usingGamepad)
            RotateWithGamepad();
        else
            RotateWithMouse();
    }

    private void RotateWithGamepad()
    {
        Vector3 dir = new Vector3(lookInput.x, 0f, lookInput.y);
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void RotateWithMouse()
    {
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 300f, walkAreaLayer))
            RotateTowards(hit.point);
    }

    private void RotateTowards(Vector3 worldPoint)
    {
        Vector3 dir = worldPoint - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}
