using UnityEngine;

public class ItemBehaviour : MonoBehaviour
{
    public virtual void PrimaryInput() { }
    public virtual void SecondaryInput() { }

    public virtual void OnEquip(Transform rayOrigin) { }
    public virtual void OnUnequip() 
    {
        Destroy(gameObject);
    }
}
