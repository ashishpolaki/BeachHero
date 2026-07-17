using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class PlayerPreviewEditTool : MonoBehaviour
    {
        // local-space control points relative to this transform
        public List<Vector3> pathPoints = new List<Vector3>()
        {
        };
        public float previewSpeed = 5f;
        public const float FixedDeltaTime = 1f / 60f;
        [HideInInspector] public float previewPercent = 0f;

        [Header("Freehand / Sampling")]
        public bool enforceYZero = true;
        public bool freehandEnabled = true;
        [SerializeField] private float freehandSpacing = 0.3f; // now private (not editable in UI)
        public float evenlySpacing = 0.5f;
        [SerializeField] private float previewDuration = 10f; // private duration used to compute time (not editable in scene UI)

        public float GetPreviewDuration() => previewDuration;
        public float GetFreehandSpacing() => freehandSpacing;

        private Transform player;

        public void SetPlayerTransform(Transform player)
        {
            this.player = player;
        }

        private void ResetPlayerPosition()
        {
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
        }

        // Move the player along the path (percent 0..1)
        public void UpdatePreview(float percent)
        {
            if (player == null) return;
            previewPercent = Mathf.Clamp01(percent);
            if (pathPoints == null || pathPoints.Count < 2 || player == null)
            {
                ResetPlayerPosition();
                return;
            }
            // linear mapping across segments
            int segmentCount = pathPoints.Count - 1;
            float total = previewPercent * segmentCount;
            int idx = Mathf.Clamp(Mathf.FloorToInt(total), 0, segmentCount - 1);
            float t = total - idx;

            Vector3 aLocal = pathPoints[idx];
            Vector3 bLocal = pathPoints[idx + 1];

            Vector3 worldPos = transform.TransformPoint(Vector3.Lerp(aLocal, bLocal, t));
            if (enforceYZero) worldPos.y = 0f;

            player.transform.position = worldPos;

            // face movement direction
            Vector3 forward = (transform.TransformPoint(bLocal) - transform.TransformPoint(aLocal)).normalized;
            if (forward != Vector3.zero)
            {
                Quaternion target = Quaternion.LookRotation(new Vector3(forward.x, 0f, forward.z));
                player.transform.rotation = target;
            }
        }

        // Simple helper: total linear length (world-space)
        public float CalculateTotalLength()
        {
            if (pathPoints == null || pathPoints.Count < 2) return 0f;
            float len = 0f;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                Vector3 a = transform.TransformPoint(pathPoints[i]);
                Vector3 b = transform.TransformPoint(pathPoints[i + 1]);
                len += Vector3.Distance(a, b);
            }
            return len;
        }
        public float GetTotalDuration()
        {
            float len = CalculateTotalLength();
            if (len <= 0f || previewSpeed <= 0f) return 0f;
            return len / previewSpeed;
        }
        public void AdvancePreviewByFixedStep(bool forward)
        {
            float len = CalculateTotalLength();
            if (len <= 0f) return;

            float deltaPercent = (previewSpeed * FixedDeltaTime) / len;
            previewPercent = Mathf.Clamp01(previewPercent + (forward ? deltaPercent : -deltaPercent));
            UpdatePreview(previewPercent);
        }
        public void ClearPoints()
        {
            pathPoints?.Clear();
        }
    }
}