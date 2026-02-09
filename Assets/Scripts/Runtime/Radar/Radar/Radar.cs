using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.RadarSystem
{
    [AddComponentMenu("Radar System/Submarine Sonar")]
    public class SubmarineSonar : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField, Min(1f)]
        private float detectionRange = 200f;

        [SerializeField]
        private LayerMask pointOfInterestLayers;

        [Header("Radar Visual")]
        [SerializeField]
        private Transform radarVisual;

        [SerializeField]
        private float radarRadius = 0.5f;

        [Header("Blips")]
        [SerializeField]
        private GameObject blipPrefab;

        [SerializeField]
        private float blipHeight = 0.05f;

        private readonly Dictionary<Transform, Transform> blips = new();

        private void Update()
        {
            ScanAndUpdateBlips();
        }

        private void ScanAndUpdateBlips()
        {
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                detectionRange,
                pointOfInterestLayers
            );

            HashSet<Transform> detectedThisFrame = new();

            foreach (var hit in hits)
            {
                Transform poi = hit.transform;
                detectedThisFrame.Add(poi);

                if (!blips.ContainsKey(poi))
                {
                    CreateBlip(poi);
                }

                UpdateBlipPosition(poi);
            }

            // Remove blips that are no longer detected
            List<Transform> toRemove = new();
            foreach (var pair in blips)
            {
                if (!detectedThisFrame.Contains(pair.Key))
                {
                    Destroy(pair.Value.gameObject);
                    toRemove.Add(pair.Key);
                }
            }

            foreach (var key in toRemove)
                blips.Remove(key);
        }

        private void CreateBlip(Transform poi)
        {
            GameObject blip = Instantiate(blipPrefab, radarVisual);
            blip.name = $"Blip_{poi.name}";
            blips.Add(poi, blip.transform);
        }

        private void UpdateBlipPosition(Transform poi)
        {
            Transform blip = blips[poi];

            Vector3 localDirection =
                poi.position - transform.position;

            // Ignore vertical difference for radar
            localDirection.y = 0f;

            float distance = localDirection.magnitude;
            float normalizedDistance = Mathf.Clamp01(distance / detectionRange);

            // Convert direction to local radar space
            Vector3 radarSpaceDir =
                radarVisual.InverseTransformDirection(localDirection.normalized);

            // Radius mapping (far = outer edge)
            float radius = normalizedDistance * radarRadius;

            Vector3 localPos = new Vector3(
                radarSpaceDir.x * radius,
                blipHeight,
                radarSpaceDir.z * radius
            );

            blip.localPosition = localPos;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}