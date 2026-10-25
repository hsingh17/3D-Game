using System;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public static Action<float> OnThrowStartEvent;
    public static Action OnThrowReleaseEvent;

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
    private Item itemInHand;
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
    }

    private void OnDisable()
    {
        InputReader.OnThrowStartEvent -= StartThrow;
        InputReader.OnThrowReleaseEvent -= Throw;
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
            if (interactable is Item item)
            {
                GrabItem(item);
            }
            else
            {
                interactable.Interact(gameObject);
            }
        }
    }

    private void GrabItem(Item item)
    {
        if (!itemInHand)
        {
            item.Interact(gameObject);
            itemInHand = item;
        }
        else
        {
            inventory.PutItemInInventory(item);
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
            itemInHand = null;
            OnThrowReleaseEvent?.Invoke();
        }
    }
}
