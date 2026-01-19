using UnityEngine;

public class Building : MonoBehaviour
{
    private BuildingData _assignedData;
    public BuildingData AssignedData => _assignedData;
    public BoxCollider Col;

    private Renderer _renderer;
    private Material _defaultMaterial;

    private bool _flaggedForDelete;
    public bool FlaggedForDelete => _flaggedForDelete;

    private int _collisionCount = 0;
    public bool IsColliding() => _collisionCount > 0;

    public void Init(BuildingData data)
    {
        _assignedData = data;
        _renderer = GetComponent<Renderer>();
        if(_defaultMaterial == null && _renderer) _defaultMaterial = _renderer.material;
        gameObject.layer = 2;
        if(!TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        if (TryGetComponent<BoxCollider>(out Col))
        {
            Col.isTrigger = true;
        }
    }

    public void PlaceBuilding()
    {
        UpdateMaterial(_defaultMaterial);
        Col.isTrigger = false;
        gameObject.layer = 15;

        var rb = GetComponent<Rigidbody>();
        Destroy(rb);
    }

    public void UpdateMaterial(Material newMaterial)
    {
        if(_renderer.material != newMaterial) _renderer.material = newMaterial;
    }

    public void FlagForDelete(Material deleteMat)
    {
        UpdateMaterial(deleteMat);
        _flaggedForDelete = true;
    }

    public void RemoveDeleteFlag(){
        UpdateMaterial(_defaultMaterial);
        _flaggedForDelete = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        _collisionCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;
        _collisionCount--;
    }

    private void OnDisable()
    {
        _collisionCount = 0;
    }
}
