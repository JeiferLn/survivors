using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private CharacterController controller;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float gravity = -9.81f;
    private float verticalVelocity;

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform cameraTransform;

    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

        controller = GetComponent<CharacterController>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        Movement();
    }

    void FixedUpdate()
    {
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
        Vector3 lookDir = cameraTransform.forward;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }
}
