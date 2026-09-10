using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class HudStaminaController : MonoBehaviour
{
    private ProgressBar progressBar;

    private float maxValue;

    private void OnEnable() => GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);

    private void OnDisable() =>
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);

    private void OnUIReload(PanelRenderer pr, VisualElement root)
    {
        progressBar = root.Q<ProgressBar>("stamina");
    }

    public void UpdateStaminaValueByProportion(float proportion)
    {
        progressBar.value = progressBar.highValue * proportion;
    }
}
