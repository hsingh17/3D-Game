using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class HudStaminaController : MonoBehaviour
{
    private ProgressBar progressBar;

    private void OnEnable() => GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);

    private void OnDisable() =>
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);

    private void OnUIReload(PanelRenderer pr, VisualElement root)
    {
        progressBar = root.Q<ProgressBar>("stamina");
    }

    public void UpdateStaminaValue(float value) => progressBar.value = value;
}
