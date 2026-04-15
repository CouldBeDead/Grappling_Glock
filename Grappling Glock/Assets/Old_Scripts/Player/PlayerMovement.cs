using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Assignables
    public Transform playerCam;
    public Transform orientation;

    // Other
    private Rigidbody rb;

    // Rotation and look
    private float xRotation;
    private float desiredX;

    [Header("Mouse Settings")]
    public float sensitivity = 50f;
    public float sensMultiplier = 1f;

    // Movement
    [Header("Movement")]
    public float moveSpeed = 4500f;
    public float maxSpeed = 100f;
    public bool grounded;
    public LayerMask whatIsGround;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    // Crouch & Slide
    [Header("Crouch / Slide")]
    private Vector3 crouchScale = new Vector3(1f, 0.5f, 1f);
    private Vector3 playerScale;
    public float slideForce = 400f;
    public float slideCounterMovement = 0.1f;

    // Jumping
    [Header("Jumping")]
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;

    // Input
    private float x;
    private float y;
    private bool jumping;
    private bool crouching;

    // Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    [Header("Sliding Audio")]
    public AudioSource slideAudio;
    public float maxSlideVolume = 1.5f;
    public float minSpeedForAudio = 0.1f;

    private bool cancellingGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (orientation != null)
            desiredX = orientation.transform.eulerAngles.y;
    }

    private void FixedUpdate()
    {
        Movement();
        UpdateSlideAudio();
    }

    private void Update()
    {
        MyInput();
        Look();
    }

    private void MyInput()
    {
        Keyboard kb = Keyboard.current;

        x = 0f;
        y = 0f;
        jumping = false;
        crouching = false;

        if (kb == null)
            return;

        // WASD movement
        if (kb.aKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed) x += 1f;
        if (kb.sKey.isPressed) y -= 1f;
        if (kb.wKey.isPressed) y += 1f;

        x = Mathf.Clamp(x, -1f, 1f);
        y = Mathf.Clamp(y, -1f, 1f);

        // Jump / crouch
        jumping = kb.spaceKey.isPressed;
        crouching = kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed;

        // Crouch start / stop
        if (kb.leftCtrlKey.wasPressedThisFrame || kb.rightCtrlKey.wasPressedThisFrame)
            StartCrouch();

        if (kb.leftCtrlKey.wasReleasedThisFrame || kb.rightCtrlKey.wasReleasedThisFrame)
            StopCrouch();
    }

    private void StartCrouch()
    {
        transform.localScale = crouchScale;
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y - 0.5f,
            transform.position.z
        );

        if (rb.linearVelocity.magnitude > 0.5f && grounded)
        {
            Vector3 slideDirection = orientation.transform.forward;
            float boostStrength = 25f;
            rb.AddForce(slideDirection * boostStrength, ForceMode.Impulse);
            grounded = false;
        }

        if (slideAudio != null && !slideAudio.isPlaying)
            slideAudio.Play();
    }

    private void StopCrouch()
    {
        transform.localScale = playerScale;
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + 0.5f,
            transform.position.z
        );

        if (slideAudio != null)
            slideAudio.Stop();
    }

    private void UpdateSlideAudio()
    {
        if (slideAudio == null)
            return;

        float speed = rb.linearVelocity.magnitude;

        if (crouching && grounded && speed > minSpeedForAudio)
        {
            slideAudio.volume = Mathf.Clamp01(speed / maxSpeed) * maxSlideVolume;
        }
        else
        {
            slideAudio.volume = 0f;
        }
    }

    private void Movement()
    {
        rb.AddForce(Vector3.down * Time.deltaTime * 10f);

        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x;
        float yMag = mag.y;

        CounterMovement(x, y, mag);

        if (readyToJump && jumping)
            Jump();

        float currentMaxSpeed = maxSpeed;

        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000f);
            return;
        }

        if (x > 0 && xMag > currentMaxSpeed) x = 0;
        if (x < 0 && xMag < -currentMaxSpeed) x = 0;
        if (y > 0 && yMag > currentMaxSpeed) y = 0;
        if (y < 0 && yMag < -currentMaxSpeed) y = 0;

        float multiplier = 1f;
        float multiplierV = 1f;

        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        if (grounded && crouching)
            multiplierV = 0f;

        rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump()
    {
        if (grounded && readyToJump)
        {
            readyToJump = false;

            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            Vector3 vel = rb.linearVelocity;
            if (rb.linearVelocity.y < 0.5f)
                rb.linearVelocity = new Vector3(vel.x, 0f, vel.z);
            else if (rb.linearVelocity.y > 0f)
                rb.linearVelocity = new Vector3(vel.x, vel.y / 2f, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Look()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || playerCam == null || orientation == null)
            return;

        Vector2 mouseDelta = mouse.delta.ReadValue();

        float mouseX = mouseDelta.x * sensitivity * sensMultiplier * 0.01f;
        float mouseY = mouseDelta.y * sensitivity * sensMultiplier * 0.01f;

        desiredX += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0f);
        orientation.transform.localRotation = Quaternion.Euler(0f, desiredX, 0f);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping)
            return;

        if (crouching)
        {
            if (rb.linearVelocity.sqrMagnitude > 0.0001f)
            {
                rb.AddForce(moveSpeed * Time.deltaTime * -rb.linearVelocity.normalized * slideCounterMovement);
            }
            return;
        }

        if ((Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f) || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }

        if ((Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f) || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        if (Mathf.Sqrt((Mathf.Pow(rb.linearVelocity.x, 2f) + Mathf.Pow(rb.linearVelocity.z, 2f))) > maxSpeed)
        {
            float fallSpeed = rb.linearVelocity.y;
            Vector3 n = rb.linearVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(n.x, fallSpeed, n.z);
        }
    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.linearVelocity.x, rb.linearVelocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90f - u;

        float magnitude = rb.linearVelocity.magnitude;
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private void OnCollisionStay(Collision other)
    {
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer)))
            return;

        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;

            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    public void ResetMovementState()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        x = 0f;
        y = 0f;
        jumping = false;
        crouching = false;

        grounded = false;
        readyToJump = true;

        desiredX = orientation.transform.eulerAngles.y;
        xRotation = 0f;

        playerCam.transform.localRotation = Quaternion.Euler(0f, desiredX, 0f);
        orientation.transform.localRotation = Quaternion.Euler(0f, desiredX, 0f);

        if (slideAudio != null)
        {
            slideAudio.Stop();
            slideAudio.volume = 0f;
        }
    }
}