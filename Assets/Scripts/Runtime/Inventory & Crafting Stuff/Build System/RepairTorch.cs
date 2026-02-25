using UnityEngine;

public class RepairTorch : ItemBehaviour
{
    [SerializeField] private float _rayDistance = 20;
    [SerializeField] private LayerMask _layerMask;

    public override void PrimaryInput()
    {
        RaycastHit hitInfo;
        Physics.Raycast(player.PlayerHead.position, player.PlayerHead.transform.forward, out hitInfo, _rayDistance, _layerMask);

        if (hitInfo.collider.TryGetComponent<CrackRepair>(out var crackRepair))
        {
            crackRepair.Repair();
        }
        else
        {
            Debug.Log("No crack to repair");
        }
    }
}
