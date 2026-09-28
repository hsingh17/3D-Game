using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(InputReader))]
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
    private EntityData scriptableObject;

    [SerializeField]
    private Camera camera;

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
    [Range(0f, 90f)]
    private float maxSlopeAngle;

    [SerializeField]
    [Range(0f, 5f)]
    private float slopeCastPadding;

    private InputReader inputReader;
    private PlayerStamina playerStamina;
    private CapsuleCollider collider;
    private Rigidbody rb;
    private bool evaluatingGroundCheck = false;
    private bool jumpOffCd = true;
    private RaycastHit slopeHit;

    public bool IsGrounded { get; set; }

    private void Awake()
    {
        collider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        playerStamina = GetComponent<PlayerStamina>();
        inputReader = GetComponent<InputReader>();

        rb.useGravity = false;
        IsGrounded = true;
    }

    private void Update()
    {
        UpdateStamina();
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
            IsGrounded = hit;
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
        Vector3 movementVector = scriptableObject.moveSpeed * inputReader.Move;

        // Apply sprinting if necessary
        movementVector.z *= IsSprinting() ? scriptableObject.sprintMultiplier : 1;

        // Rotate movement vector to align with the "forward" direction of the player
        movementVector = rb.rotation * movementVector;

        if (OnSlope())
        {
            movementVector = Vector3.ProjectOnPlane(movementVector, slopeHit.normal);
        }

        rb.AddForce(movementVector, ForceMode.VelocityChange);
        rb.linearDamping = IsGrounded ? scriptableObject.groundDrag : scriptableObject.airDrag;
    }

    private void ApplyGravity()
    {
        if (!IsGrounded)
        {
            rb.AddForce(
                Physics.gravity * scriptableObject.gravityMultiplier,
                ForceMode.Acceleration
            );
        }
    }

    private void Jump()
    {
        if (IsGrounded && jumpOffCd && inputReader.Jump > 0)
        {
            rb.AddForce(
                scriptableObject.jumpForce * inputReader.Jump * Vector3.up,
                ForceMode.VelocityChange
            );
            jumpOffCd = false;
            StartCoroutine(JumpCooldown());
        }
    }

    private void Look()
    {
        RotatePlayer();
    }

    private void RotatePlayer()
    {
        Vector3 curRotationEulerAngles = rb.rotation.eulerAngles;
        rb.rotation = Quaternion.Euler(
            curRotationEulerAngles.x,
            curRotationEulerAngles.y + (inputReader.Look.x * GameSettings.Instance().Sensitivity.x),
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
        IsGrounded = DidHitGround();
    }

    private void UpdateStamina()
    {
        if (IsSprinting())
        {
            playerStamina.UseStamina(scriptableObject.sprintStaminaUsagePerSec * Time.deltaTime);
        }
    }

    private bool OnSlope()
    {
        bool onSlope = Physics.Raycast(
            transform.TransformPoint(collider.center),
            Vector3.down,
            out slopeHit,
            collider.height / 2 + slopeCastPadding
        );
        float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
        return onSlope && angle != 0 && angle <= maxSlopeAngle;
    }

    public bool IsWalkingForward() => (!IsSprinting()) && inputReader.Move.z > 0 && IsGrounded;

    public bool IsWalkingBackward() => (!IsSprinting()) && inputReader.Move.z < 0 && IsGrounded;

    public bool IsSprinting() =>
        inputReader.Sprint > 0
        && inputReader.Move.z > 0
        && IsGrounded
        && playerStamina.CanUseStamina();
}
