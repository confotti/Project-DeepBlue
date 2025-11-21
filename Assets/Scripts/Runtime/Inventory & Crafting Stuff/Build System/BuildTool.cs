using UnityEngine;
using UnityEngine.InputSystem;

namespace BuildSystem
{
    public class BuildTool : MonoBehaviour
    {
        [SerializeField] private float _rotateSnapAngle = 45f;
        [SerializeField] private float _rayDistance = 20;
        [SerializeField] private LayerMask _buildModeLayerMask;
        [SerializeField] private LayerMask _deleteModeLayerMask;
        [SerializeField] private int _defaultLayerInt = 15;
        [SerializeField] private Transform _rayOrigin;
        [SerializeField] private Material _buildingMatPositive;
        [SerializeField] private Material _buildingMatNegative;

        private bool _deleteModeEnabled = false;

        private Camera cam;

        [SerializeField] private Building _spawnedBuilding;
        private Building _targetBuilding;
        private Quaternion _lastRotation;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if (Keyboard.current.qKey.wasPressedThisFrame) _deleteModeEnabled = !_deleteModeEnabled;

            if (_deleteModeEnabled) DeleteModeLogic();
            else BuildModeLogic();

        }

        private void ChoosePart(BuildingData data)
        {
            if (_deleteModeEnabled)
            {
                UnassignTargetBuilding();
                _deleteModeEnabled = false;
            }

            if (_spawnedBuilding != null)
            {
                ObjectPoolManager.ReturnObjectToPool(_spawnedBuilding.gameObject);
                _spawnedBuilding = null;
            }

            _spawnedBuilding = ObjectPoolManager.Instantiate(data.Prefab);
            _spawnedBuilding.Init(data);
            _spawnedBuilding.transform.rotation = _lastRotation;
        }

        private bool IsRayHittingSomething(LayerMask layerMask, out RaycastHit hitInfo)
        {
            return Physics.Raycast(_rayOrigin.position, cam.transform.forward, out hitInfo, _rayDistance, layerMask);
        }

        private void BuildModeLogic()
        {
            UnassignTargetBuilding();

            if (_spawnedBuilding == null) return;

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                _spawnedBuilding.transform.Rotate(0, _rotateSnapAngle, 0);
                _lastRotation = _spawnedBuilding.transform.rotation;
            }

            if (!IsRayHittingSomething(_buildModeLayerMask, out RaycastHit hitInfo))
            {
                //_spawnedBuilding.UpdateMaterial(_buildingMatNegative);
                _spawnedBuilding.gameObject.SetActive(false);
            }
            else
            {
                _spawnedBuilding.gameObject.SetActive(true);
                _spawnedBuilding.transform.position = hitInfo.point + new Vector3(0, _spawnedBuilding.Col.size.y / 2, 0);

                if (Physics.OverlapBox(_spawnedBuilding.transform.position + _spawnedBuilding.Col.center, _spawnedBuilding.Col.size / 2, _spawnedBuilding.transform.rotation).Length > 0)
                {
                    _spawnedBuilding.UpdateMaterial(_buildingMatNegative);
                    return;
                }

                _spawnedBuilding.UpdateMaterial(_buildingMatPositive);

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _spawnedBuilding.PlaceBuilding();
                    var dataCopy = _spawnedBuilding.AssignedData;
                    _spawnedBuilding = null;
                    ChoosePart(dataCopy);
                    
                }
            }

        }

        private void DeleteModeLogic()
        {
            if (!IsRayHittingSomething(_deleteModeLayerMask, out RaycastHit hitInfo)) 
            {
                UnassignTargetBuilding();
                return;
            }

            var detectedBuilding = hitInfo.collider.gameObject.GetComponentInParent<Building>();

            if (detectedBuilding == null)
            {
                UnassignTargetBuilding();
                return;
            }

            if (_targetBuilding == null) _targetBuilding = detectedBuilding;

            if (detectedBuilding != _targetBuilding && _targetBuilding.FlaggedForDelete)
            {
                _targetBuilding.RemoveDeleteFlag();
                _targetBuilding = detectedBuilding;
            }

            if(detectedBuilding == _targetBuilding && !_targetBuilding.FlaggedForDelete)
            {
                _targetBuilding.FlagForDelete(_buildingMatNegative);

            }

            if (Mouse.current.leftButton.wasPressedThisFrame) 
            {
                ObjectPoolManager.ReturnObjectToPool(_targetBuilding.gameObject);
                UnassignTargetBuilding();
            } 
        }

        private void UnassignTargetBuilding()
        {
            if (_targetBuilding != null && _targetBuilding.FlaggedForDelete)
            {
                _targetBuilding.RemoveDeleteFlag();
            }
            _targetBuilding = null;
        }
    }
}
