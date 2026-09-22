using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [Range(-1f, 1f)]
    private float stepCheckPadding;

    [SerializeField]
    [Range(0f, 1f)]
    private float maxStepHeight;

    [SerializeField]
    [Range(0f, 1f)]
    private float stepCheckDistance;

    [SerializeField]
    [Range(0f, 1f)]
    private float stepSmoothing;

    [SerializeField]
    private PlayerAnimator playerAnimator;

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
            collider.radius,
            Vector3.down,
            out _,
            (collider.height / 2) - collider.radius + groundCheckPadding,
            ground.value
        );
    }

    private void Move()
    {
        if (MoveUpStep())
        {
            rb.MovePosition(rb.position + new Vector3(0, stepSmoothing, 0));
        }
        else
        {
            Vector3 delta = scriptableObject.moveSpeed * move;

            // Apply sprinting if necessary
            delta.z *= IsSprinting() ? scriptableObject.sprintMultiplier : 1;

            // Rotate our movement delta vector to align with the "forward" direction of the player
            delta = rb.rotation * delta;
            rb.AddForce(delta, ForceMode.VelocityChange);
            rb.linearDamping = isGrounded ? scriptableObject.groundDrag : scriptableObject.airDrag;
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
        else if (!isGrounded)
        {
            rb.AddForce(
                Physics.gravity * scriptableObject.gravityMultiplier,
                ForceMode.Acceleration
            );
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

    private bool MoveUpStep()
    {
        Dictionary<Vector3, StairHitCheck> stepChecks = CheckSteps();
        foreach (var (dir, hitCheck) in stepChecks)
        {
            if (
                hitCheck.DidHitStair
                && (
                    (dir == Vector3.forward && move.z > 0)
                    || (dir == Vector3.back && move.z < 0)
                    || (dir == Vector3.left && move.x < 0)
                    || (dir == Vector3.right && move.x > 0)
                )
            )
            {
                return true;
            }
        }
        return false;
    }

    private Dictionary<Vector3, StairHitCheck> CheckSteps()
    {
        return new()
        {
            [Vector3.forward] = CheckStepInDirection(transform.forward),
            [Vector3.back] = CheckStepInDirection(Quaternion.Euler(0, 180, 0) * transform.forward),
            [Vector3.left] = CheckStepInDirection(Quaternion.Euler(0, -90, 0) * transform.forward),
            [Vector3.right] = CheckStepInDirection(Quaternion.Euler(0, 90, 0) * transform.forward),
        };
    }

    private StairHitCheck CheckStepInDirection(Vector3 direction)
    {
        float distToFeet = collider.height / 2 + stepCheckPadding;
        Vector3 playerFeet = transform.TransformPoint(collider.center) + distToFeet * Vector3.down;
        Ray bottomRay = new(playerFeet, direction);
        Ray topRay = new(playerFeet + (maxStepHeight * Vector3.up), direction);

        Debug.DrawRay(bottomRay.origin, bottomRay.direction);
        Debug.DrawRay(topRay.origin, topRay.direction);

        bool didBottomHit = Physics.Raycast(
            bottomRay,
            out RaycastHit bottomHit,
            stepCheckDistance,
            ground.value
        );

        bool didTopHit = Physics.Raycast(
            topRay,
            out RaycastHit topHit,
            stepCheckDistance,
            ground.value
        );

        return new(new(didTopHit, topHit), new(didBottomHit, bottomHit));
    }

    private void UpdatePlayerState()
    {
        if (DidInitiateJump())
        {
            // TODO: Fix this since there's not enough time between starting jump and falling
            playerAnimator.SetState("JumpStart");
        }
        else if (IsLanding())
        {
            // TODO: Fix this since there's not enough time between falling and landing
            playerAnimator.SetState("Land");
        }
        else if (!isGrounded)
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

    private bool IsLanding() => isGrounded && playerAnimator.CurrentState == "Falling";

    private bool DidInitiateJump() => jump > 0 && isGrounded;

    private bool IsWalkingForward() => (!IsSprinting()) && move.z > 0 && isGrounded;

    private bool IsWalkingBackward() => (!IsSprinting()) && move.z < 0 && isGrounded;

    private bool IsSprinting() =>
        sprint > 0 && move.z > 0 && isGrounded && playerStamina.CanUseStamina();
}
