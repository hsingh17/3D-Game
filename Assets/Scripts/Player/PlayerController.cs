using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private EntityScriptableObject scriptableObject;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 move;
    private float jump;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

    }

    private void Update()
    {
        move = moveAction.ReadValue<Vector2>();
        jump = jumpAction.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        Move();
        Jump();
    }


    private void Move()
    {
        Debug.Log(move);
    }

    private void Jump()
    {
        Debug.Log(jump);
    }
}
