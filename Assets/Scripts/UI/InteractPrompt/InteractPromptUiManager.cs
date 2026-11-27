using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class InteractPromptUiManager : MonoBehaviour
{
    private Label interactPromptTextLabel;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
        Hand.OnDetectInteractable += UpdateInteractPromptTextLabel;
    }

    private void OnDisable()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
        Hand.OnDetectInteractable -= UpdateInteractPromptTextLabel;
    }

    private void OnUIReload(PanelRenderer pr, VisualElement root)
    {
        interactPromptTextLabel = root.Q<Label>("interact-prompt-text");
    }

    private void UpdateInteractPromptTextLabel(IInteractable interactable, Vector3 interactPosition)
    {
        if (interactable != null)
        {
            string interactBinding = InputReader.Instance.InteractBinding;
            interactPromptTextLabel.text = interactable is InteractableItem item
                ? $"[{interactBinding}] Pick up {item.ItemInstance.ItemData.ItemName}"
                : $"[{interactBinding}] Interact with {interactable}"; // TODO: Interactable name
            interactPromptTextLabel.visible = true;

            Vector3 screenPoint = RuntimePanelUtils.CameraTransformWorldToPanel(
                interactPromptTextLabel.panel,
                interactPosition,
                mainCamera
            );
            interactPromptTextLabel.style.left = screenPoint.x;
            interactPromptTextLabel.style.top = screenPoint.y;
        }
        else
        {
            interactPromptTextLabel.visible = false;
        }
    }
}
