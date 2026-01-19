using UnityEngine;
using UnityEngine.InputSystem;

namespace BuildSystem
{
    public class BuildTool : ItemBehaviour
    {
        [SerializeField] private float _rotateSnapAngle = 45f;
        [SerializeField] private float _rayDistance = 20;
        [SerializeField] private LayerMask _buildModeLayerMask;
        [SerializeField] private LayerMask _deleteModeLayerMask;
        [SerializeField] private Material _buildingMatPositive;
        [SerializeField] private Material _buildingMatNegative;

        private bool _deleteModeEnabled = false;

        private Camera cam;

        private Building _spawnedBuilding;
        private Building _targetBuilding;
        private Quaternion _lastRotation;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void OnEnable()
        {
            BuildDisplay.OnPartChosen += ChoosePart;
        }

        private void OnDisable()
        {
            BuildDisplay.OnPartChosen -= ChoosePart;
        }

        public override void OnEquip(PlayerItemHandler player)
        {
            base.OnEquip(player);
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            if (_spawnedBuilding != null) ObjectPoolManager.ReturnObjectToPool(_spawnedBuilding.gameObject);
        }

        private void Update()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame) BuildDisplay.OnBuildDisplayRequested?.Invoke();
            if (Keyboard.current.qKey.wasPressedThisFrame) _deleteModeEnabled = !_deleteModeEnabled;


            ClearCostUI();
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

            _spawnedBuilding = ObjectPoolManager.SpawnObject(data.Prefab);
            _spawnedBuilding.Init(data);
            _spawnedBuilding.transform.rotation = _lastRotation;
        }

        private bool IsRayHittingSomething(LayerMask layerMask, out RaycastHit hitInfo)
        {
            return Physics.Raycast(player.PlayerHead.position, cam.transform.forward, out hitInfo, _rayDistance, layerMask);
        }

        private void BuildModeLogic()
        {
            UnassignTargetBuilding();

            if (_spawnedBuilding == null) return;

            UpdateCostUI();

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
                _spawnedBuilding.transform.position = hitInfo.point
                    + new Vector3(0, 0.1f + _spawnedBuilding.Col.size.y * _spawnedBuilding.transform.lossyScale.y / 2)
                    - _spawnedBuilding.Col.center * _spawnedBuilding.transform.lossyScale.y;

                if (_spawnedBuilding.IsColliding() || !CheckIfCanAffordCurrentBuilding())
                {
                    _spawnedBuilding.UpdateMaterial(_buildingMatNegative);
                    return;
                }

                _spawnedBuilding.UpdateMaterial(_buildingMatPositive);

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    PayCurrentBuildingCost();
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

            if (detectedBuilding == _targetBuilding && !_targetBuilding.FlaggedForDelete)
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

        private void UpdateCostUI()
        {
            //TODO: Fix this function
        }

        private void ClearCostUI()
        {
            //TODO: Fix this function
        }

        private bool CheckIfCanAffordCurrentBuilding()
        {
            foreach (ItemCost cost in _spawnedBuilding.AssignedData.Cost)
            {
                if (player.PlayerInventory.InventorySystem.AmountOfItem(cost.ItemRequired) < cost.AmountRequired)
                {
                    return false;
                }
            }

            return true;
        }

        private void PayCurrentBuildingCost()
        {
            foreach (ItemCost cost in _spawnedBuilding.AssignedData.Cost)
            {
                player.PlayerInventory.RemoveItemFromInventory(cost);
            }
        }
    }
}
