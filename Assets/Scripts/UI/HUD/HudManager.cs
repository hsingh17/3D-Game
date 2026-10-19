using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class HudManager : MonoBehaviour
{
    private ProgressBar staminaBar;
    private ProgressBar throwChargeBar;
    private static HudManager instance;

    private void Awake()
    {
        instance = instance != null ? instance : this;
    }

    private void OnEnable() => GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);

    private void OnDisable() =>
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);

    private void OnUIReload(PanelRenderer pr, VisualElement root)
    {
        staminaBar = root.Q<ProgressBar>("stamina");
        throwChargeBar = root.Q<ProgressBar>("throw-charge");
    }

    private HudManager() { }

    public static void UpdateStaminaValueByProportion(float proportion)
    {
        if (instance != null)
        {
            instance.staminaBar.value = instance.staminaBar.highValue * proportion;
        }
    }
}
