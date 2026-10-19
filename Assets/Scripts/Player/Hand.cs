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
    [Range(1f, 200f)]
    private float maxThrowForce;

    [SerializeField]
    [Range(1f, 10f)]
    private float maxThrowChargeDuration;

    private bool detectedInteractable;
    private RaycastHit interactableRayCastInfo;
    private Item itemInHand;
    private Camera camera;

    public float MaxThrowChargeDuration() => maxThrowChargeDuration;

    private void Awake()
    {
        camera = GetComponentInParent<Camera>();
        inputReader.OnThrowEvent += Throw;
    }

    private void OnDestroy()
    {
        inputReader.OnThrowEvent -= Throw;
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
            if (interactable is Item item)
            {
                if (!itemInHand)
                {
                    item.Interact(gameObject);
                    itemInHand = item;
                }
            }
            else
            {
                interactable.Interact(gameObject);
            }
        }
    }

    private void Throw(double duration)
    {
        if (itemInHand)
        {
            float force = (float)(
                maxThrowForce
                / maxThrowChargeDuration
                * Math.Clamp(duration, 0, maxThrowChargeDuration)
            );
            itemInHand.Throw(camera.transform.forward * force);
            itemInHand = null;
        }
    }
}
