using UnityEngine;

public class ItemBehaviour : MonoBehaviour
{
    public virtual void PrimaryInput() { }

    public virtual void SecondaryInput() { }

    public virtual void Init(Transform rayOrigin) { }
}
