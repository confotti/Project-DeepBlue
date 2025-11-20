using UnityEngine;
using UnityEngine.InputSystem;

namespace BuildSystem
{
    public class BuildTool : MonoBehaviour
    {
        [SerializeField] private float _rayDistance = 20;
        [SerializeField] private LayerMask _buildModeLayerMask;
        [SerializeField] private LayerMask _deleteModeLayerMask;
        [SerializeField] private int _defaultLayerInt = 9;
        [SerializeField] private Transform _rayOrigin;
        [SerializeField] private Material _buildingMatPositive;
        [SerializeField] private Material _buildingMatNegative;

        private bool deleteModeEnabled = false;

        private Camera cam;

        [SerializeField] private Building _spawnedBuilding;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if (Keyboard.current.qKey.wasPressedThisFrame) deleteModeEnabled = !deleteModeEnabled;

            if (deleteModeEnabled) DeleteModeLogic();
            else BuildModeLogic();

        }

        private bool IsRayHittingSomething(LayerMask layerMask, out RaycastHit hitInfo)
        {
            return Physics.Raycast(_rayOrigin.position, cam.transform.forward, out hitInfo, _rayDistance, layerMask);
        }

        private void BuildModeLogic()
        {
            if (_spawnedBuilding == null) return;

            if (!IsRayHittingSomething(_buildModeLayerMask, out RaycastHit hitInfo))
            {
                _spawnedBuilding.UpdateMaterial(_buildingMatNegative);
            }
            else
            {
                _spawnedBuilding.UpdateMaterial(_buildingMatPositive);
                _spawnedBuilding.transform.position = hitInfo.point;
            }

            _spawnedBuilding.transform.position = hitInfo.point;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Building placedBuilding = ObjectPoolManager.SpawnObject(_spawnedBuilding, hitInfo.point);
                placedBuilding.PlaceBuilding();
                //Continue from 9:00. 
            } 
        }

        private void DeleteModeLogic()
        {
            if(!IsRayHittingSomething(_deleteModeLayerMask, out RaycastHit hitInfo)) return;

            if (Mouse.current.leftButton.wasPressedThisFrame) ObjectPoolManager.ReturnObjectToPool(hitInfo.collider.gameObject);
        }
    }
}
