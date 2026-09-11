using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerStamina : MonoBehaviour
{
    [SerializeField]
    private EntityScriptableObject scriptableObject;

    [SerializeField]
    private HudStaminaController hudStaminaController;

    [SerializeField]
    private float regenWaitTimeSeconds;

    private float timeSinceLastStaminaConsumingAction;
    private bool blockStaminaUsage;

    public float MaxStamina { get; set; }
    public float CurrentStamina { get; set; }

    public void UseStamina(float consumedStamina)
    {
        if (CurrentStamina > 0 && !blockStaminaUsage)
        {
            CurrentStamina = Mathf.Clamp(CurrentStamina - consumedStamina, 0, MaxStamina);
            timeSinceLastStaminaConsumingAction = 0;
            blockStaminaUsage = CurrentStamina == 0;
            hudStaminaController.UpdateStaminaValueByProportion(CurrentStamina / MaxStamina);
        }
    }

    public bool CanUseStamina() => !blockStaminaUsage && CurrentStamina > 0;

    private void Awake()
    {
        CurrentStamina = scriptableObject.maxStamina;
        MaxStamina = scriptableObject.maxStamina;
        timeSinceLastStaminaConsumingAction = 0;
        blockStaminaUsage = false;
    }

    private void Update()
    {
        RegenStamina();
    }

    private void RegenStamina()
    {
        timeSinceLastStaminaConsumingAction = Mathf.Clamp(
            timeSinceLastStaminaConsumingAction + Time.deltaTime,
            0,
            regenWaitTimeSeconds
        );

        if (
            CurrentStamina < MaxStamina
            && timeSinceLastStaminaConsumingAction >= regenWaitTimeSeconds
        )
        {
            CurrentStamina = Mathf.Clamp(
                CurrentStamina + scriptableObject.staminaRegenPerSec * Time.deltaTime,
                0,
                MaxStamina
            );

            if (CurrentStamina == MaxStamina && blockStaminaUsage)
            {
                blockStaminaUsage = false;
            }

            hudStaminaController.UpdateStaminaValueByProportion(CurrentStamina / MaxStamina);
        }
    }
}
