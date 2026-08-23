using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerStamina : MonoBehaviour
{
    [SerializeField]
    private EntityScriptableObject scriptableObject;

    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private HudStaminaController hudStaminaController;

    private InputAction sprintAction;

    public bool CanSprint { get; set; }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        sprintAction = playerInput.actions["Sprint"];
    }

    private void Update()
    {
        float sprint = sprintAction.ReadValue<float>();
    }
}
