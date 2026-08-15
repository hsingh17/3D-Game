using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
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

    private CapsuleCollider collider;
    private Rigidbody rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private Vector3 move;
    private Vector2 look;
    private float jump;
    private float sprint;
    private float pitch;
    private bool isGrounded = true;
    private bool jumpOffCd = true;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        collider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        lookAction = playerInput.actions["Look"];
        sprintAction = playerInput.actions["Sprint"];
        rb.useGravity = false;
    }

    private void Update()
    {
        ReadActionInputs();
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
            transform.position,
            collider.radius,
            Vector3.down,
            out _,
            (collider.height / 2) - collider.radius + groundCheckPadding,
            ground.value
        );
    }

    private void Move()
    {
        Vector3 forwardAbsolute = new(
            Mathf.Abs(transform.forward.x),
            Mathf.Abs(transform.forward.y),
            Mathf.Abs(transform.forward.z)
        );
        Vector3 delta = transform.rotation * (scriptableObject.moveSpeed * move);
        Vector3 sprintVector =
            sprint > 0 ? scriptableObject.sprintMultiplier * forwardAbsolute : Vector3.zero;
        sprintVector += new Vector3(1, 0, 1);
        delta = Vector3.Scale(delta, sprintVector);

        rb.AddForce(delta, ForceMode.VelocityChange);
        rb.linearDamping = isGrounded ? scriptableObject.groundDrag : scriptableObject.airDrag;
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
        Vector3 curRotationEulerAngles = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(
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
}
