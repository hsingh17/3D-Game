using System;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public static Action<float> OnThrowStartEvent;
    public static Action<ItemInstance> OnThrowReleaseEvent;

    [SerializeField]
    private LayerMask layersToCheck;

    [SerializeField]
    [Range(0, 10f)]
    private float maxInteractDistance;

    [SerializeField]
    [Range(1f, 200f)]
    private float maxThrowForce;

    [SerializeField]
    [Range(1f, 10f)]
    private float maxThrowChargeDuration;

    private bool detectedInteractable;
    private RaycastHit interactableRayCastInfo;
    private InteractableItem itemInHand;
    private Camera camera;
    private Inventory inventory;

    public float MaxThrowChargeDuration() => maxThrowChargeDuration;

    private void Awake()
    {
        camera = GetComponentInParent<Camera>();
        TryGetComponent(out inventory);
    }

    private void OnEnable()
    {
        InputReader.OnThrowStartEvent += StartThrow;
        InputReader.OnThrowReleaseEvent += Throw;
        InputReader.OnItemSlotSelect += SwapItemInHandByIdx;
        InputReader.OnItemSlotScroll += SwapItemInHandByDelta;
    }

    private void OnDisable()
    {
        InputReader.OnThrowStartEvent -= StartThrow;
        InputReader.OnThrowReleaseEvent -= Throw;
        InputReader.OnItemSlotSelect -= SwapItemInHandByIdx;
        InputReader.OnItemSlotScroll -= SwapItemInHandByDelta;
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
        if (!detectedInteractable || InputReader.Instance.Interact <= 0)
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
            if (interactable is InteractableItem item)
            {
                GrabItem(item);
            }
            else
            {
                interactable.Interact(gameObject);
            }
        }
    }

    private void GrabItem(InteractableItem item)
    {
        int idx = inventory.AddItem(item);
        // Item was put into inventory succesfully.
        if (idx == -1)
        {
            return;
        }
        else if (!itemInHand)
        {
            item.Interact(gameObject);
            itemInHand = item;
        }
        else
        {
            // Item went into inventory so remove it from scene.
            Destroy(item.gameObject);
        }
    }

    private void StartThrow()
    {
        if (itemInHand)
        {
            OnThrowStartEvent?.Invoke(maxThrowChargeDuration);
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
            inventory.RemoveItem(itemInHand);
            OnThrowReleaseEvent?.Invoke(itemInHand.ItemInstance);
            itemInHand = null;
        }
    }

    private void SwapItemInHandByDelta(float delta)
    {
        SwapItemInHand(inventory.GetItemByDelta(delta));
    }

    private void SwapItemInHandByIdx(int idx)
    {
        SwapItemInHand(inventory.GetItem(idx));
    }

    private void SwapItemInHand(InteractableItem item)
    {
        // Destroy old item in hand
        if (itemInHand)
        {
            Destroy(itemInHand.gameObject);
        }

        // Even if item returned is null, we still want to show player with nothing in hand
        itemInHand = item;
        if (item)
        {
            item.Interact(gameObject);
        }
    }
}
