using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    [RequireComponent(typeof(Player))]
    public class PlayerPreviewEditTool : MonoBehaviour
    {
        // local-space control points relative to this transform
        public List<Vector3> pathPoints = new List<Vector3>()
        {
            new Vector3(0,0,0),
            new Vector3(5,0,0),
            new Vector3(10,0,5)
        };

        [HideInInspector] public float previewPercent = 0f;

        [Header("Freehand / Sampling")]
        public bool enforceYZero = true;
        public bool freehandEnabled = true;
        [SerializeField] private float freehandSpacing = 0.25f; // now private (not editable in UI)

        [SerializeField] private float previewDuration = 10f; // private duration used to compute time (not editable in scene UI)

        private Player player;

        private void Awake()
        {
            player = GetComponent<Player>();
        }

        // Move the player along the path (percent 0..1)
        public void UpdatePreview(float percent)
        {
            previewPercent = Mathf.Clamp01(percent);
            if (pathPoints == null || pathPoints.Count < 2 || player == null) return;

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

        // Editor helpers (world-space)
        public void AddPointWorld(Vector3 worldPoint)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            if (enforceYZero) local.y = 0f;
            if (pathPoints == null) pathPoints = new List<Vector3>();
            pathPoints.Add(local);
        }

        public void InsertPointWorld(int index, Vector3 worldPoint)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            if (enforceYZero) local.y = 0f;
            if (pathPoints == null) pathPoints = new List<Vector3>();
            index = Mathf.Clamp(index, 0, pathPoints.Count);
            pathPoints.Insert(index, local);
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

        // expose helpers for editor read-only values
        public float GetPreviewDuration() => previewDuration;
        public float GetFreehandSpacing() => freehandSpacing;

        public void ClearPoints()
        {
            pathPoints?.Clear();
        }
    }
}