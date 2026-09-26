using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputReader : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    public Vector3 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public float Sprint { get; private set; }
    public float Jump { get; private set; }

    private void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        lookAction = playerInput.actions["Look"];
        sprintAction = playerInput.actions["Sprint"];
    }

    private void Update()
    {
        Vector2 movement = moveAction.ReadValue<Vector2>();
        Jump = jumpAction.ReadValue<float>();
        Move = new(movement.x, 0, movement.y);
        Look = lookAction.ReadValue<Vector2>();
        Sprint = sprintAction.ReadValue<float>();
    }
}
