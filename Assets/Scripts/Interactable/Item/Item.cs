using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    [SerializeField]
    private GameObject itemsParentGameObject;

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
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
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
        gameObject.transform.SetParent(itemsParentGameObject.transform);
    }
}
