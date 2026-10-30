using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class HudManager : MonoBehaviour
{
    public static HudManager Instance;
    private ProgressBar staminaBar;
    private ProgressBar throwChargeBar;
    private bool chargingThrow;
    private float timeStartChargingThrow;

    private HudManager() { }

    private void Awake()
    {
        Instance = Instance != null ? Instance : this;
        chargingThrow = false;
    }

    private void OnEnable()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
        PlayerStamina.OnStaminaUpdate += UpdateStaminaUI;
        Hand.OnThrowStartEvent += StartThrow;
        Hand.OnThrowReleaseEvent += EndThrow;
    }

    private void OnDisable()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
        Hand.OnThrowStartEvent -= StartThrow;
        Hand.OnThrowReleaseEvent -= EndThrow;
    }

    private void Update()
    {
        UpdateThrowCharge();
    }

    private void OnUIReload(PanelRenderer pr, VisualElement root)
    {
        staminaBar = root.Q<ProgressBar>("stamina");
        throwChargeBar = root.Q<ProgressBar>("throw-charge");
        throwChargeBar.visible = false;
    }

    private void UpdateStaminaUI(float currentStamina, float maxStamina)
    {
        staminaBar.value = currentStamina;
        staminaBar.highValue = maxStamina;
    }

    private void StartThrow(float maxThrowChargeDuration)
    {
        throwChargeBar.value = 0;
        throwChargeBar.highValue = maxThrowChargeDuration;
        chargingThrow = true;
        timeStartChargingThrow = Time.time;
        throwChargeBar.visible = true;
    }

    private void UpdateThrowCharge()
    {
        if (chargingThrow && throwChargeBar.value < throwChargeBar.highValue)
        {
            throwChargeBar.value = Time.time - timeStartChargingThrow;
        }
    }

    private void EndThrow(Item _)
    {
        chargingThrow = false;
        throwChargeBar.visible = false;
    }
}
