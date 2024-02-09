using UnityEngine;

public class Interactable : MonoBehaviour
{
    public InteractableType InteractableType;
    public virtual void StartInteract(Player playerManager)
    {

    }
    public virtual void Interact(Player playerManager)
    {

    }
    public virtual void EndInteract(Player playerManager) { }
}
