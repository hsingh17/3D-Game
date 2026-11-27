using UnityEngine;

public class InteractableItem : MonoBehaviour, IInteractable
{
    // The only reason we have this is so inventory can keep a reference to the item
    // safely since Destroy is called on InteractableItem
    public ItemInstance ItemInstance;
    private Rigidbody rb;

    public bool IsInteractable { get; set; }

    private void Awake()
    {
        IsInteractable = true;
        TryGetComponent(out rb);
    }

    public virtual void Interact(GameObject interactor)
    {
        IsInteractable = false;

        // Disable physics of the picked up item
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // Move object to target position
        gameObject.transform.SetParent(interactor.transform);
        transform.SetLocalPositionAndRotation(
            ItemInstance.ItemData.InHandLocalPosition,
            Quaternion.Euler(ItemInstance.ItemData.InHandLocalRotation)
        );
    }

    public virtual void Use()
    {
        Debug.Log("Used item");
    }

    public virtual void Throw(Vector3 force)
    {
        IsInteractable = true;
        // Re-enable physics of the picked up item
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        rb.AddForce(force, ForceMode.Impulse);

        // Move object back to the "items" game object
        gameObject.transform.SetParent(ItemInstance.ItemsParentGameObject.transform);
    }

    public void PostInstantiate(ItemData itemData, GameObject parentGameObj)
    {
        ItemInstance = new(itemData, parentGameObj);
        parentGameObj.transform.SetParent(parentGameObj.transform);
    }
}
