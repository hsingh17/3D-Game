using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
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

    private Rigidbody rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private Vector3 move;
    private Vector2 look;
    private float pitch;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        lookAction = playerInput.actions["Look"];
    }

    private void Update()
    {
        Vector2 movement = moveAction.ReadValue<Vector2>();
        float jump = jumpAction.ReadValue<float>();
        move = new(movement.x, jump, movement.y);
        look = lookAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Look();
        Move();
    }

    private void Move()
    {
        Debug.Log(move.y);
        Vector3 delta = transform.rotation * (scriptableObject.moveSpeed * Time.deltaTime * move);
        rb.MovePosition(transform.position + delta);
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
}
