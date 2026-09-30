using UnityEngine;

[RequireComponent(typeof(InputReader))]
public class InteractableHandler : MonoBehaviour
{
    [SerializeField]
    private LayerMask interactableLayer;

    [SerializeField]
    [Range(0, 10f)]
    private float maxInteractDistance;

    private InputReader inputReader;

    [SerializeField]
    private Camera camera;

    private bool detectedInteractable;
    private RaycastHit interactableRayCastInfo;

    private void Awake()
    {
        inputReader = GetComponent<InputReader>();
    }

    private void FixedUpdate()
    {
        CheckForInteractable();
        Interact();
    }

    private void CheckForInteractable()
    {
        detectedInteractable = Physics.Raycast(
            camera.transform.position,
            camera.transform.forward,
            out interactableRayCastInfo,
            maxInteractDistance,
            interactableLayer
        );
    }

    private void Interact()
    {
        if (!detectedInteractable || inputReader.Interact <= 0)
        {
            return;
        }

        GameObject interactedGameObj = interactableRayCastInfo.collider.gameObject;
        if (interactedGameObj == null)
        {
            return;
        }

        if (interactedGameObj.TryGetComponent(out IInteractable interactable))
        {
            interactable.Interact();
        }
    }
}
