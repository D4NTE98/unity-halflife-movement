using UnityEngine;

[RequireComponent(typeof(Rigidbody), RequireComponent(typeof(CapsuleCollider))]
public class CSMovement : MonoBehaviour
{
    // Ustawienia ruchu
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float airControl = 0.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float friction = 5f;

    // Bunnyhop
    [Header("Bunnyhop Settings")]
    [SerializeField] private float bunnyHopThreshold = 0.1f;
    [SerializeField] private float bunnyHopMultiplier = 1.1f;

    // Crouch
    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchSmoothTime = 0.1f;

    // Referencje
    private Rigidbody rb;
    private CapsuleCollider col;
    private bool isGrounded;
    private bool isCrouching;
    private float currentHeight;
    private Vector3 wishDir;
    private float verticalVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        currentHeight = standHeight;
    }

    void Update()
    {
        HandleInput();
        GroundCheck();
        Crouch();
    }

    void FixedUpdate()
    {
        Movement();
        ApplyFriction();
    }

    private void HandleInput()
    {
        // Input A/D i W/S
        wishDir = (transform.right * Input.GetAxisRaw("Horizontal") + 
                 transform.forward * Input.GetAxisRaw("Vertical")).normalized;

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // Bunnyhop
        if (Input.GetButton("Jump") && 
            !isGrounded && 
            verticalVelocity < bunnyHopThreshold)
        {
            BunnyHop();
        }
    }

    private void GroundCheck()
    {
        float sphereRadius = col.radius * 0.9f;
        Vector3 spherePos = transform.position + Vector3.up * (sphereRadius - 0.1f);
        isGrounded = Physics.CheckSphere(spherePos, sphereRadius, 
            LayerMask.GetMask("Ground"));
    }

    private void Movement()
    {
        float speed = isCrouching ? crouchSpeed : walkSpeed;
        Vector3 moveForce;

        if (isGrounded)
        {
            moveForce = wishDir * speed * 10f;
        }
        else
        {
            // Air strafing
            float dot = Vector3.Dot(rb.velocity.normalized, wishDir);
            moveForce = wishDir * speed * 10f * airControl * (1 - Mathf.Abs(dot));
        }

        rb.AddForce(moveForce);

        // Ground strafe
        if (isGrounded && wishDir.magnitude > 0)
        {
            Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            horizontalVel = Vector3.ClampMagnitude(horizontalVel, speed);
            rb.velocity = new Vector3(horizontalVel.x, rb.velocity.y, horizontalVel.z);
        }

        verticalVelocity = rb.velocity.y;
    }

    private void ApplyFriction()
    {
        if (isGrounded && wishDir.magnitude < 0.1f)
        {
            Vector3 horizontalVel = rb.velocity;
            horizontalVel.y = 0;
            horizontalVel *= Mathf.Clamp01(1 - friction * Time.fixedDeltaTime);
            rb.velocity = new Vector3(horizontalVel.x, rb.velocity.y, horizontalVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void BunnyHop()
    {
        Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        horizontalVel *= bunnyHopMultiplier;
        rb.velocity = new Vector3(horizontalVel.x, rb.velocity.y, horizontalVel.z);
    }

    private void Crouch()
    {
        isCrouching = Input.GetKey(KeyCode.LeftControl);

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, 
            crouchSmoothTime * Time.deltaTime);

        col.height = currentHeight;
        col.center = Vector3.up * (currentHeight * 0.5f);
    }
}
