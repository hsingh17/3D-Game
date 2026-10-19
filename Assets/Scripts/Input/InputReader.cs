using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputReader : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction throwAction;
    public Vector3 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public float Sprint { get; private set; }
    public float Jump { get; private set; }
    public float Interact { get; private set; }

    public Action<double> OnThrowEvent;

    private void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        lookAction = playerInput.actions["Look"];
        sprintAction = playerInput.actions["Sprint"];
        interactAction = playerInput.actions["Interact"];
        throwAction = playerInput.actions["Throw"];
        throwAction.performed += ChargeThrow;
        throwAction.canceled += EndThrow;
    }

    private void OnDestroy()
    {
        throwAction.canceled -= EndThrow;
    }

    private void Update()
    {
        Vector2 movement = moveAction.ReadValue<Vector2>();
        Jump = jumpAction.ReadValue<float>();
        Move = new(movement.x, 0, movement.y);
        Look = lookAction.ReadValue<Vector2>();
        Sprint = sprintAction.ReadValue<float>();
        Interact = interactAction.ReadValue<float>();
    }

    private void ChargeThrow(InputAction.CallbackContext ctx) => Debug.Log(ctx.duration);

    private void EndThrow(InputAction.CallbackContext ctx) => OnThrowEvent?.Invoke(ctx.duration);
}
