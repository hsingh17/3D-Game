using System;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField]
    private LayerMask layersToCheck;

    [SerializeField]
    [Range(0, 10f)]
    private float maxInteractDistance;

    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    [Range(0, 100f)]
    private float minThrowForce;

    [SerializeField]
    [Range(0, 200f)]
    private float maxThrowForce;

    private bool detectedInteractable;
    private RaycastHit interactableRayCastInfo;
    private Item itemInHand;
    private Camera camera;

    private void Awake()
    {
        camera = GetComponentInParent<Camera>();
    }

    private void FixedUpdate()
    {
        CheckForInteractable();
        Interact();
        Throw();
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
            // Special logic if the interactable is an item
            if (interactable is Item item && !itemInHand)
            {
                item.Interact(gameObject);
                itemInHand = item;
            }
            else
            {
                interactable.Interact(gameObject);
            }
        }
    }

    private void Throw()
    {
        if (!itemInHand || inputReader.Throw <= 0)
        {
            return;
        }

        // TODO: This should range between min and max depending how long user held throw button for
        itemInHand.Throw(camera.transform.forward * minThrowForce);
        itemInHand = null;
    }
}
