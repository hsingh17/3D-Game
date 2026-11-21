using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputReader : MonoBehaviour
{
    public static InputReader Instance;
    public static Action OnThrowStartEvent;
    public static Action<double> OnThrowReleaseEvent;
    public static Action<int> OnItemSlotSelect;
    public static Action<float> OnItemSlotScroll;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction throwAction;
    private InputAction selectItemSlotAction;
    private InputAction scrollItemSlotAction;
    public Vector3 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public float Sprint { get; private set; }
    public float Jump { get; private set; }
    public float Interact { get; private set; }
    public int SelectItemSlot { get; private set; }
    public int ScrollItemSlot { get; private set; }

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
        selectItemSlotAction = playerInput.actions["SelectItemSlot"];
        scrollItemSlotAction = playerInput.actions["ScrollSlot"];
    }

    private void OnEnable()
    {
        throwAction.performed += StartThrow;
        throwAction.canceled += EndThrow;
        selectItemSlotAction.performed += OnPerformSelectItemSlot;
        scrollItemSlotAction.performed += OnScrollItemSlot;
    }

    private void OnDisable()
    {
        throwAction.performed -= StartThrow;
        throwAction.canceled -= EndThrow;
        selectItemSlotAction.performed -= OnPerformSelectItemSlot;
        scrollItemSlotAction.performed -= OnScrollItemSlot;
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

    private void OnScrollItemSlot(InputAction.CallbackContext ctx)
    {
        float val = ctx.ReadValue<float>();
        if (val != 0)
        {
            OnItemSlotScroll?.Invoke(val * -1);
        }
    }

    private void OnPerformSelectItemSlot(InputAction.CallbackContext ctx)
    {
        if (int.TryParse(ctx.control.displayName, out int numKey))
        {
            OnItemSlotSelect?.Invoke(MapToZeroBasedIdx(numKey));
        }
    }

    private int MapToZeroBasedIdx(int numKey)
    {
        if (numKey == 0)
        {
            return 9;
        }
        else
        {
            return numKey - 1;
        }
    }
}
