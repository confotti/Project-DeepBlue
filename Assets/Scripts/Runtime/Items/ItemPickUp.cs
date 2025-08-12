using UnityEngine;

public class ItemPickUp : MonoBehaviour, IInteractable
{
    [SerializeField] private Pickups itemType;

    public void Interact()
    {
        Debug.Log($"Picked up {itemType}");
        Destroy(gameObject);
    }
}

public enum Pickups
{
    wood,
    plastic
}
