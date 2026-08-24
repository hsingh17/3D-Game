using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerStamina : MonoBehaviour
{
    [SerializeField]
    private EntityScriptableObject scriptableObject;

    [SerializeField]
    private HudStaminaController hudStaminaController;

    public float MaxStamina { get; set; }
    public float CurrentStamina { get; set; }

    private void Awake()
    {
        CurrentStamina = scriptableObject.maxStamina;
        MaxStamina = scriptableObject.maxStamina;
    }

    public void UseStamina(float consumedStamina)
    {
        if (CurrentStamina > 0)
        {
            CurrentStamina = Mathf.Clamp(CurrentStamina - consumedStamina, 0, MaxStamina);
            hudStaminaController.UpdateStaminaValueByProportion(CurrentStamina / MaxStamina);
            Debug.Log(CurrentStamina);
        }
    }

    public void RegenStamina()
    {
        if (CurrentStamina < MaxStamina)
        {
            CurrentStamina = Mathf.Clamp(
                CurrentStamina + scriptableObject.staminaRegenPerSec * Time.deltaTime,
                0,
                MaxStamina
            );
        }
    }
}
