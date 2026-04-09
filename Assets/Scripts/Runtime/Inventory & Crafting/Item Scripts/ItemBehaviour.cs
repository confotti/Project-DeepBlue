using UnityEngine;

public class ItemBehaviour : MonoBehaviour
{
    protected PlayerItemHandler _player;

    [SerializeField] private Transform _holdingPoint;
    public Transform HoldingPoint => _holdingPoint;

    public virtual void PrimaryInput() { }
    public virtual void SecondaryInput() { }

    public virtual void OnEquip(PlayerItemHandler player)
    {
        _player = player;
    }
    public virtual void OnUnequip() 
    {
        Destroy(gameObject);
    }
}
