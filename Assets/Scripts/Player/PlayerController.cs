using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerStamina))]
public class PlayerController : MonoBehaviour
{
    public struct HitCheck
    {
        public bool DidHit { get; set; }
        public RaycastHit Hit { get; set; }

        public HitCheck(bool didHit, RaycastHit hit)
        {
            DidHit = didHit;
            Hit = hit;
        }
    }

    public struct StairHitCheck
    {
        public bool DidHitStair { get; set; }
        public HitCheck TopHit { get; set; }
        public HitCheck BottomHit { get; set; }

        public StairHitCheck(HitCheck topHit, HitCheck bottomHit)
        {
            TopHit = topHit;
            BottomHit = bottomHit;
            DidHitStair = bottomHit.DidHit && !topHit.DidHit;
        }
    }

    [SerializeField]
    private EntityScriptableObject scriptableObject;

    [SerializeField]
    private Camera camera;

    [SerializeField]
    private MouseSensitivity mouseSensitivity;

    [SerializeField]
    private float pitchMin;

    [SerializeField]
    private float pitchMax;

    [SerializeField]
    private LayerMask ground;

    [SerializeField]
    [Range(0f, 10f)]
    private float jumpCooldownSeconds;

    [SerializeField]
    [Range(0f, 1f)]
    private float groundCheckPadding;

    [SerializeField]
    [Range(0f, 2f)]
    private float minUngroundedTimeSeconds;

    [SerializeField]
    private PlayerAnimator playerAnimator;

    [SerializeField]
    [Range(0f, 90f)]
    private float maxSlopeAngle;

    [SerializeField]
    [Range(0f, 5f)]
    private float slopeCastPadding;

    private PlayerStamina playerStamina;
    private CapsuleCollider collider;
    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private Vector3 move;
    private Vector2 look;
    private float sprint;
    private float jump;
    private float pitch;
    private bool evaluatingGroundCheck = false;
    private bool isGrounded = true;
    private bool jumpOffCd = true;
    private RaycastHit slopeHit;

    private void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        collider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        playerStamina = GetComponent<PlayerStamina>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        lookAction = playerInput.actions["Look"];
        sprintAction = playerInput.actions["Sprint"];
        rb.useGravity = false;
    }

    private void Update()
    {
        ReadActionInputs();
        UpdateStamina();
        UpdatePlayerState();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        Look();
        Move();
        Jump();
        ApplyGravity();
    }

    private void CheckGrounded()
    {
        bool hit = DidHitGround();
        if (hit)
        {
            isGrounded = hit;
        }
        else if (!hit && !evaluatingGroundCheck)
        {
            StartCoroutine(MinimumUngroundedTime());
        }
    }

    private bool DidHitGround()
    {
        return Physics.SphereCast(
            transform.TransformPoint(collider.center),
            collider.radius / 2,
            Vector3.down,
            out _,
            (collider.height / 2) - collider.radius + groundCheckPadding,
            ground.value
        );
    }

    private void Move()
    {
        Vector3 movementVector = scriptableObject.moveSpeed * move;

        // Apply sprinting if necessary
        movementVector.z *= IsSprinting() ? scriptableObject.sprintMultiplier : 1;

        // Rotate movement vector to align with the "forward" direction of the player
        movementVector = rb.rotation * movementVector;

        if (OnSlope())
        {
            // If on slope, then cast movement vector onto that slope
            movementVector = Vector3.ProjectOnPlane(movementVector, slopeHit.normal);
            // TODO: Add downward force when moving down plane to keep player from bouncing on slope
            // rb.AddForce(Vector3.down * 80f, ForceMode.Acceleration);
        }

        rb.AddForce(movementVector, ForceMode.VelocityChange);
        rb.linearDamping = isGrounded ? scriptableObject.groundDrag : scriptableObject.airDrag;
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(
                Physics.gravity * scriptableObject.gravityMultiplier,
                ForceMode.Acceleration
            );
        }
    }

    private void Jump()
    {
        if (isGrounded && jumpOffCd && jump > 0)
        {
            rb.AddForce(scriptableObject.jumpForce * jump * Vector3.up, ForceMode.VelocityChange);
            jumpOffCd = false;
            StartCoroutine(JumpCooldown());
        }
    }

    private void Look()
    {
        RotatePlayer();
        RotateCamera();
    }

    private void RotatePlayer()
    {
        Vector3 curRotationEulerAngles = rb.rotation.eulerAngles;
        rb.rotation = Quaternion.Euler(
            curRotationEulerAngles.x,
            curRotationEulerAngles.y + (look.x * mouseSensitivity.x),
            curRotationEulerAngles.z
        );
    }

    private void RotateCamera()
    {
        Vector3 curRotationEulerAngles = camera.transform.rotation.eulerAngles;
        pitch = Mathf.Clamp(pitch + (-look.y * mouseSensitivity.y), pitchMin, pitchMax);
        camera.transform.rotation = Quaternion.Euler(
            pitch,
            curRotationEulerAngles.y,
            curRotationEulerAngles.z
        );
    }

    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldownSeconds);
        jumpOffCd = true;
    }

    private IEnumerator MinimumUngroundedTime()
    {
        evaluatingGroundCheck = true;
        yield return new WaitForSeconds(minUngroundedTimeSeconds);
        evaluatingGroundCheck = false;
        isGrounded = DidHitGround();
    }

    private void ReadActionInputs()
    {
        Vector2 movement = moveAction.ReadValue<Vector2>();
        jump = isGrounded ? jumpAction.ReadValue<float>() : 0;
        move = new(movement.x, 0, movement.y);
        look = lookAction.ReadValue<Vector2>();
        sprint = sprintAction.ReadValue<float>();
    }

    private void UpdatePlayerState()
    {
        if (!isGrounded)
        {
            playerAnimator.SetState("Falling");
        }
        else if (IsSprinting())
        {
            playerAnimator.SetState("Sprint");
        }
        else if (IsWalkingForward())
        {
            playerAnimator.SetState("WalkForward");
        }
        else if (IsWalkingBackward())
        {
            playerAnimator.SetState("WalkBackward");
        }
        else
        {
            playerAnimator.SetState("Idle");
        }
    }

    private void UpdateStamina()
    {
        if (IsSprinting())
        {
            playerStamina.UseStamina(scriptableObject.sprintStaminaUsagePerSec * Time.deltaTime);
        }
    }

    private bool IsWalkingForward() => (!IsSprinting()) && move.z > 0 && isGrounded;

    private bool IsWalkingBackward() => (!IsSprinting()) && move.z < 0 && isGrounded;

    private bool IsSprinting() =>
        sprint > 0 && move.z > 0 && isGrounded && playerStamina.CanUseStamina();

    private bool OnSlope()
    {
        bool onSlope = Physics.Raycast(
            transform.TransformPoint(collider.center),
            Vector3.down,
            out RaycastHit slopeHit,
            collider.height / 2 + slopeCastPadding
        );
        float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
        return onSlope && angle != 0 && angle <= maxSlopeAngle;
    }
}
