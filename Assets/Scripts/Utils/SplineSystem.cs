using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    [System.Serializable]
    public class SplineSystem
    {
        [SerializeField] private List<SplinePoint> points = new List<SplinePoint>();
        [Range(5, 100)] public int resolution = 20;

        // ===== PROPERTIES =====
        public List<SplinePoint> Points => points;
        public int Count => points != null ? points.Count : 0;
        public bool IsValid => points != null && points.Count >= 4;

        #region Methods
        public Vector3 GetTangent(float percent)
        {
            percent = Mathf.Clamp01(percent);

            if (points == null || points.Count < 4)
                return Vector3.forward;

            List<Vector3> pts = GetPositions();
            return CatmullSplineUtils.GetTangentOnSpline(pts, percent);
        }

        public float CalculateDistance(float startPercent, float endPercent)
        {
            var pts = Points;
            if (pts.Count < 4) return 0f;
            float distance = 0f;
            int steps = resolution * (pts.Count - 3); // same density as draw
            float prevT = startPercent;
            Vector3 prev = GetPoint(prevT);
            for (int i = 1; i <= steps; i++)
            {
                float lerpT = i / (float)steps;
                float t = Mathf.Lerp(startPercent, endPercent, lerpT);
                Vector3 p = GetPoint(t);
                distance += Vector3.Distance(prev, p);
                prev = p;
            }
            return distance;
        }

        public Quaternion GetForwardRotation(float percent, bool isForward = true)
        {
            float safePercent = Mathf.Clamp01(percent);
            safePercent = Mathf.Min(safePercent, 0.98f);

            Vector3 dir = GetTangent(percent);

            if (dir == Vector3.zero)
                return Quaternion.identity;

            if (!isForward)
                dir = -dir;

            return Quaternion.LookRotation(dir, Vector3.back);
        }

        public Quaternion GetTwistRotation(float percent)
        {
            percent = Mathf.Clamp01(percent);

            int count = points.Count;
            if (count < 2)
                return Quaternion.identity;

            float scaled = percent * (count - 1);
            int i = Mathf.FloorToInt(scaled);
            float t = scaled - i;

            i = Mathf.Clamp(i, 0, count - 2);

            Quaternion a = points[i].rotation;
            Quaternion b = points[i + 1].rotation;

            if (Quaternion.Dot(a, b) < 0f)
            {
                b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
            }

            Quaternion rot = Quaternion.Slerp(a, b, t);
            return rot;
        }

        public Vector3 GetPoint(float percent)
        {
            percent = Mathf.Clamp01(percent);

            if (points == null || points.Count < 4)
                return Vector3.zero;

            List<Vector3> pts = GetPositions();
            return CatmullSplineUtils.GetPointOnSpline(pts, percent);
        }

        public List<Vector3> GetPositions()
        {
            List<Vector3> pts = new List<Vector3>();
            for (int i = 0; i < points.Count; i++)
                pts.Add(points[i].position);
            return pts;
        }
        #endregion
    }
}
