using UnityEngine;

public class ItemBehaviour : MonoBehaviour
{
    protected PlayerItemHandler player;

    public virtual void PrimaryInput() { }
    public virtual void SecondaryInput() { }

    public virtual void OnEquip(PlayerItemHandler player)
    {
        this.player = player;
    }
    public virtual void OnUnequip() 
    {
        Destroy(gameObject);
    }
}
