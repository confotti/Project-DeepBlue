using UnityEngine;

[CreateAssetMenu(menuName = "Equipment Effects/Swim Speed Effect")]
public class SwimSpeedEquipmentEffect : EquipmentEffectsBase
{
    public float slowSpeedModifier = 4;
    public float fastSpeedModifier = 5;


    //TODO: Very cursed way to get to PlayerMovement, fix later. 
    public override void ApplyEquipmentEffect(PlayerStats stats)
    {
        stats.GetComponent<PlayerMovement>().SwimmingState.SetSwimSpeedsModifiers(slowSpeedModifier, fastSpeedModifier);
    }

    public override void RemoveEquipmentEffect(PlayerStats stats)
    {
        stats.GetComponent<PlayerMovement>().SwimmingState.SetSwimSpeedsModifiers(0);
    }
}
