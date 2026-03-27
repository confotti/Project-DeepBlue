using UnityEngine;

public class ItemBehaviour : MonoBehaviour
{
    protected PlayerItemHandler _player;

    public Vector3 relativeUp = new Vector3(0, 1, 0);
    public Vector3 holdingPosition = new Vector3(0, 0, 0);

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
