using UnityEngine;

public interface IInteractable
{
    bool IsInteractable { get; set; }

    void Interact(GameObject interactor);
}
