using System.Collections;
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
    private float jumpCooldownSeconds;

    [SerializeField]
    private float groundCheckPadding;

    [SerializeField]
    private float stepCheckPadding;

    [SerializeField]
    private float maxStepHeight;

    [SerializeField]
    private float stepCheckDistance;

    [SerializeField]
    private float stepSmoothing;

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
    private bool isGrounded = true;
    private bool jumpOffCd = true;

    private void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        collider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        playerStamina = GetComponent<PlayerStamina>();
        playerAnimator = GetComponent<PlayerAnimator>();
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
        isGrounded = Physics.SphereCast(
            rb.position,
            collider.radius,
            Vector3.down,
            out _,
            (collider.height / 2) - collider.radius + groundCheckPadding,
            ground.value
        );
    }

    private void Move()
    {
        var (bottomHit, topHit) = CheckSteps();
        bool doMoveUpStep = bottomHit.DidHit && !topHit.DidHit && move.z > 0;

        if (doMoveUpStep)
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

    private void ReadActionInputs()
    {
        Vector2 movement = moveAction.ReadValue<Vector2>();
        jump = isGrounded ? jumpAction.ReadValue<float>() : 0;
        move = new(movement.x, 0, movement.y);
        look = lookAction.ReadValue<Vector2>();
        sprint = sprintAction.ReadValue<float>();
    }

    private (HitCheck bottomHit, HitCheck topHit) CheckSteps()
    {
        float distToFeet = collider.height / 2 + stepCheckPadding;
        Vector3 playerFeet = rb.position + (distToFeet * Vector3.down);
        Ray bottomRay = new(playerFeet, transform.forward);
        Ray topRay = new(playerFeet + (maxStepHeight * Vector3.up), transform.forward);

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

        return (new(didBottomHit, bottomHit), new(didTopHit, topHit));
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
        else if (IsWalking())
        {
            playerAnimator.SetState("Walk");
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

    private bool IsWalking() => (!IsSprinting()) && move.z > 0 && isGrounded;

    private bool IsSprinting() =>
        sprint > 0 && move.z > 0 && isGrounded && playerStamina.CanUseStamina();
}
