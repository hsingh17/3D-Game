using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private EntityScriptableObject scriptableObject;

    [SerializeField]
    private MouseSensitivity mouseSensitivity;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private Vector3 move;
    private Vector2 look;

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
        Move();
        Look();
    }

    private void Move()
    {
        Vector3 delta = scriptableObject.moveSpeed * Time.deltaTime * move;
        rb.MovePosition(transform.position + delta);
    }

    private void Look()
    {
        transform.rotation = Quaternion.Euler(
            transform.rotation.x + (look.x * mouseSensitivity.x),
            transform.rotation.y + (look.y * mouseSensitivity.y),
            0
        );
    }
}
