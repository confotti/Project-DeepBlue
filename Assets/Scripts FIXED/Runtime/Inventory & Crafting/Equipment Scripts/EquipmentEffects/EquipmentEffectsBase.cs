using UnityEngine;

public abstract class EquipmentEffectsBase : ScriptableObject
{
    public abstract void ApplyEquipmentEffect(PlayerStats stats);

    public abstract void RemoveEquipmentEffect(PlayerStats stats);
}
