using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public bool IsInteractable { get; set; }

    private void Awake()
    {
        IsInteractable = true;
    }

    public virtual void Interact()
    {
        // The real logic for item pick up is in Hand.cs
        IsInteractable = false;
    }

    public virtual void Use()
    {
        Debug.Log("Used item");
    }
}
