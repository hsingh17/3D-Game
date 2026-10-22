using System.Collections;
using UnityEngine;

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
    private EntityData entityData;

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
        Vector3 movementVector = entityData.moveSpeed * InputReader.Instance.Move;

        // Apply sprinting if necessary
        movementVector.z *= IsSprinting() ? entityData.sprintMultiplier : 1;

        // Rotate movement vector to align with the "forward" direction of the player
        movementVector = rb.rotation * movementVector;

        if (OnSlope())
        {
            movementVector = Vector3.ProjectOnPlane(movementVector, slopeHit.normal);
        }

        rb.AddForce(movementVector, ForceMode.VelocityChange);
        rb.linearDamping = IsGrounded ? entityData.groundDrag : entityData.airDrag;
    }

    private void ApplyGravity()
    {
        if (!IsGrounded)
        {
            rb.AddForce(Physics.gravity * entityData.gravityMultiplier, ForceMode.Acceleration);
        }
    }

    private void Jump()
    {
        if (IsGrounded && jumpOffCd && InputReader.Instance.Jump > 0)
        {
            rb.AddForce(
                entityData.jumpForce * InputReader.Instance.Jump * Vector3.up,
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
            curRotationEulerAngles.y
                + (InputReader.Instance.Look.x * GameSettings.Instance().Sensitivity.x),
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
            playerStamina.UseStamina(entityData.sprintStaminaUsagePerSec * Time.deltaTime);
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

    public bool IsWalkingForward() =>
        (!IsSprinting()) && InputReader.Instance.Move.z > 0 && IsGrounded;

    public bool IsWalkingBackward() =>
        (!IsSprinting()) && InputReader.Instance.Move.z < 0 && IsGrounded;

    public bool IsSprinting() =>
        InputReader.Instance.Sprint > 0
        && InputReader.Instance.Move.z > 0
        && IsGrounded
        && playerStamina.CanUseStamina();
}
