using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputReader : MonoBehaviour
{
    public static InputReader Instance;
    public static Action OnThrowStartEvent;
    public static Action<double> OnThrowReleaseEvent;

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

    private void Awake()
    {
        Instance = Instance != null ? Instance : this;
        PlayerInput playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        lookAction = playerInput.actions["Look"];
        sprintAction = playerInput.actions["Sprint"];
        interactAction = playerInput.actions["Interact"];
        throwAction = playerInput.actions["Throw"];
    }

    private void OnEnable()
    {
        throwAction.performed += StartThrow;
        throwAction.canceled += EndThrow;
    }

    private void OnDisable()
    {
        throwAction.performed -= StartThrow;
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

    private void StartThrow(InputAction.CallbackContext ctx) => OnThrowStartEvent?.Invoke();

    private void EndThrow(InputAction.CallbackContext ctx) =>
        OnThrowReleaseEvent?.Invoke(ctx.duration);
}
