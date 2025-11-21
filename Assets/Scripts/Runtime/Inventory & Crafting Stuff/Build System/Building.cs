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

    public void Init(BuildingData data)
    {
        _assignedData = data;
        _renderer.GetComponentInChildren<Renderer>();
        if(_renderer) _defaultMaterial = _renderer.material;

        if(TryGetComponent<BoxCollider>(out Col))
        {
            Col.enabled = false;
        }
    }

    public void PlaceBuilding()
    {
        UpdateMaterial(_defaultMaterial);
        Col.enabled = true;
        gameObject.layer = 15;
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
}
