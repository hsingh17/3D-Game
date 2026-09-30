using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        DisablePhysics();
        PutItemInHand();
    }

    private void DisablePhysics()
    {
        if (gameObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    private void PutItemInHand() { }
}
