using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

public class Hand : MonoBehaviour
{
    [SerializeField]
    private LayerMask layersToCheck;

    [SerializeField]
    [Range(0, 10f)]
    private float maxInteractDistance;

    [SerializeField]
    private Camera camera;

    [SerializeField]
    private InputReader inputReader;

    private bool detectedInteractable;
    private RaycastHit interactableRayCastInfo;
    private bool itemInHand = false;

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
            layersToCheck
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

        if (
            interactedGameObj.TryGetComponent(out IInteractable interactable)
            && interactable.IsInteractable
        )
        {
            // Interact with the object
            interactable.Interact();

            // Special logic if the interactable is an item
            if (interactable is Item item)
            {
                PickUpItem(item);
            }
        }
    }

    private void PickUpItem(Item item)
    {
        if (!itemInHand && item.gameObject.TryGetComponent(out Rigidbody rb))
        {
            itemInHand = true;

            // Disable physics of the picked up item
            rb.isKinematic = true;
            rb.detectCollisions = false;

            // Move object to hand position
            item.gameObject.transform.SetParent(transform);
            item.transform.SetLocalPositionAndRotation(
                Vector3.zero,
                Quaternion.Euler(Vector3.zero)
            );
        }
    }
}
