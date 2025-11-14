using UnityEngine;
using UnityEngine.InputSystem;

namespace BuildSystem
{
    public class BuildTool : MonoBehaviour
    {
        [SerializeField] private float rayDistance = 20;
        [SerializeField] private LayerMask buildModeLayerMask;
        [SerializeField] private LayerMask deleteModeLayerMask;
        [SerializeField] private int defaultLayerInt = 9;
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private Material buildingMatPositive;
        [SerializeField] private Material buildingMatNegative;

        private bool deleteModeEnabled = false;

        private Camera cam;

        private GameObject gameObjectToPosition;

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
            return Physics.Raycast(rayOrigin.position, cam.transform.forward, out hitInfo, rayDistance, layerMask);
        }

        private void BuildModeLogic()
        {
            if (gameObjectToPosition == null) return;
            if (!IsRayHittingSomething(buildModeLayerMask, out RaycastHit hitInfo)) return;

            gameObjectToPosition.transform.position = hitInfo.point;

            if (Mouse.current.leftButton.wasPressedThisFrame) ObjectPoolManager.SpawnObject(gameObjectToPosition, hitInfo.point);
        }

        private void DeleteModeLogic()
        {
            if(!IsRayHittingSomething(deleteModeLayerMask, out RaycastHit hitInfo)) return;

            if (Mouse.current.leftButton.wasPressedThisFrame) ObjectPoolManager.ReturnObjectToPool(hitInfo.collider.gameObject);
        }
    }
}
