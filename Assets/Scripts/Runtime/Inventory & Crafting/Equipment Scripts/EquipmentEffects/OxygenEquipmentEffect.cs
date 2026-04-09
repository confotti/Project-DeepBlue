using UnityEngine;

[CreateAssetMenu(menuName = "Equipment Effects/Oxygen Effect")]
public class OxygenEquipmentEffect : EquipmentEffectsBase
{
    public int extraOxygen = 20;


    //Should swap it from SetOxygenModifier to a better way, but not right now. 
    public override void ApplyEquipmentEffect(PlayerStats stats)
    {
        stats.SetOxygenModifier(extraOxygen);
    }

    public override void RemoveEquipmentEffect(PlayerStats stats)
    {
        stats.SetOxygenModifier(0);
    }
}
