using BeachHero;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class SplinePoint
{
    public Vector3 position;
    public Quaternion rotation = Quaternion.identity;
}
public class LevelSpline : MonoBehaviour
{
    [Header("Path Points")]
    public List<SplinePoint> pathPoints = new List<SplinePoint>();

    [Header("Settings")]
    [Range(5, 100)]
    public int resolution = 20;

    [Header("Target")]
    public Transform target;
    public Transform visualChild; // rotation applied here

    [Range(0f, 1f)]
    public float percent;

    public Vector3 GetTangent(float percent)
    {
        percent = Mathf.Clamp01(percent);

        if (pathPoints == null || pathPoints.Count < 4)
            return Vector3.forward;

        List<Vector3> pts = GetPositions();
        return CatmullSplineUtils.GetTangentOnSpline(pts, percent);
    }
    public Quaternion GetForwardRotation(float percent)
    {
        float safePercent = Mathf.Clamp01(percent);
        safePercent = Mathf.Min(safePercent, 0.98f);

        Vector3 dir = GetTangent(safePercent);

        if (dir == Vector3.zero)
            return Quaternion.identity;

        return Quaternion.LookRotation(dir, Vector3.back);
    }
    public Quaternion GetTwistRotation(float percent)
    {
        percent = Mathf.Clamp01(percent);

        int count = pathPoints.Count;
        if (count < 2)
            return Quaternion.identity;

        float scaled = percent * (count - 1);
        int i = Mathf.FloorToInt(scaled);
        float t = scaled - i;

        i = Mathf.Clamp(i, 0, count - 2);

        Quaternion a = pathPoints[i].rotation;
        Quaternion b = pathPoints[i + 1].rotation;

        return Quaternion.Slerp(a, b, t);
    }
    public Quaternion GetRotation(float percent)
    {
        float safePercent = Mathf.Clamp01(percent);
        safePercent = Mathf.Min(safePercent, 0.98f);

        Vector3 dir = GetTangent(safePercent);

        if (dir == Vector3.zero)
            return Quaternion.identity;

        Quaternion forwardRot = Quaternion.LookRotation(dir, Vector3.up);
        Quaternion twist = GetTwistRotation(safePercent);
        return forwardRot * twist;
    }
    public Vector3 GetPoint(float percent)
    {
        percent = Mathf.Clamp01(percent);

        if (pathPoints == null || pathPoints.Count < 4)
            return transform.position;

        List<Vector3> pts = GetPositions();
        return CatmullSplineUtils.GetPointOnSpline(pts, percent);
    }

    List<Vector3> GetPositions()
    {
        List<Vector3> pts = new List<Vector3>();
        for (int i = 0; i < pathPoints.Count; i++)
            pts.Add(pathPoints[i].position);
        return pts;
    }

    // ---------------------------------------
    // UPDATE TARGET (EDITOR PREVIEW)
    // ---------------------------------------
    private void OnValidate()
    {
        UpdateTarget();
    }

    public void UpdateTarget()
    {
        if (target != null && pathPoints != null && pathPoints.Count >= 4)
        {
            target.position = GetPoint(percent);

            target.rotation = GetForwardRotation(percent);

            if (visualChild != null)
                visualChild.localRotation = GetTwistRotation(percent);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelSpline))]
public class LevelSplineEditor : Editor
{
    LevelSpline spline;

    private void OnEnable()
    {
        spline = (LevelSpline)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Add Point"))
            AddPoint();

        if (GUILayout.Button("Remove Last"))
            RemovePoint();

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (GUILayout.Button("Insert Mid Points"))
            InsertMidPoints();
    }

    private void OnSceneGUI()
    {
        if (spline.pathPoints == null) return;

        DrawPoints();
        DrawCurve();
    }

    // ---------------------------------------
    // DRAW + EDIT POINTS
    // ---------------------------------------
    void DrawPoints()
    {
        for (int i = 0; i < spline.pathPoints.Count; i++)
        {
            var point = spline.pathPoints[i];

            Vector3 worldPos = spline.transform.TransformPoint(point.position);

            // POSITION HANDLE
            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                point.position = spline.transform.InverseTransformPoint(newWorldPos);
                spline.UpdateTarget();
                EditorUtility.SetDirty(spline);
            }

            // ROTATION HANDLE
            EditorGUI.BeginChangeCheck();
            Quaternion newRot = Handles.RotationHandle(point.rotation, worldPos);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Rotate Point");
                point.rotation = newRot;
                spline.UpdateTarget();
                EditorUtility.SetDirty(spline);
            }

            // LABEL
            Handles.Label(worldPos + Vector3.up * 0.3f, $"P{i}");

            // DEBUG ROTATION (YELLOW)
            Handles.color = Color.yellow;

            Vector3 forward = point.rotation * Vector3.forward;
            Vector3 up = point.rotation * Vector3.up;

            Handles.DrawLine(worldPos, worldPos + forward * 1f);
            Handles.DrawLine(worldPos, worldPos + up * 0.5f);
        }
    }

    // ---------------------------------------
    // DRAW CURVE
    // ---------------------------------------
    void DrawCurve()
    {
        if (spline.pathPoints.Count < 4)
        {
            Handles.Label(Vector3.zero, "Need at least 4 points");
            return;
        }

        Handles.color = Color.green;

        int resolution = spline.resolution;

        for (int i = 0; i < spline.pathPoints.Count - 3; i++)
        {
            Vector3 prev = spline.transform.TransformPoint(
                CatmullSplineUtils.GetPoint(
                    spline.pathPoints[i].position,
                    spline.pathPoints[i + 1].position,
                    spline.pathPoints[i + 2].position,
                    spline.pathPoints[i + 3].position,
                    0f
                )
            );

            for (int j = 1; j <= resolution; j++)
            {
                float t = j / (float)resolution;

                Vector3 p = CatmullSplineUtils.GetPoint(
                    spline.pathPoints[i].position,
                    spline.pathPoints[i + 1].position,
                    spline.pathPoints[i + 2].position,
                    spline.pathPoints[i + 3].position,
                    t
                );

                p = spline.transform.TransformPoint(p);

                Handles.DrawLine(prev, p);
                prev = p;
            }
        }

        Handles.color = Color.yellow;

        int debugSteps = spline.resolution * (spline.pathPoints.Count - 3);

        for (int i = 0; i <= debugSteps; i++)
        {
            float percent = i / (float)debugSteps;

            Vector3 pos = spline.GetPoint(percent);
            Quaternion rot = spline.GetRotation(percent);

            pos = spline.transform.TransformPoint(pos);

            float size = 0.5f;

            Vector3 forward = rot * Vector3.forward;
            Vector3 up = rot * Vector3.up;
            Vector3 right = rot * Vector3.right;

            // forward (yellow)
          //  Handles.DrawLine(pos, pos + forward * size);

            // up (cyan)
            Handles.color = Color.cyan;
            Handles.DrawLine(pos, pos + up * size * 0.7f);

            //// right (red)
            Handles.color = Color.red;
            Handles.DrawLine(pos, pos + right * size * 0.7f);

            Handles.color = Color.yellow;
        }
    }

    // ---------------------------------------
    // ADD POINT
    // ---------------------------------------
    void AddPoint()
    {
        Undo.RecordObject(spline, "Add Point");

        SplinePoint newPoint = new SplinePoint();

        if (spline.pathPoints.Count > 0)
        {
            var last = spline.pathPoints[spline.pathPoints.Count - 1];
            newPoint.position = last.position + Vector3.forward * 2f;
            newPoint.rotation = last.rotation;
        }

        spline.pathPoints.Add(newPoint);

        EditorUtility.SetDirty(spline);
    }

    // ---------------------------------------
    // REMOVE POINT
    // ---------------------------------------
    void RemovePoint()
    {
        if (spline.pathPoints.Count == 0) return;

        Undo.RecordObject(spline, "Remove Point");
        spline.pathPoints.RemoveAt(spline.pathPoints.Count - 1);
        EditorUtility.SetDirty(spline);
    }

    // ---------------------------------------
    // INSERT MID POINTS
    // ---------------------------------------
    void InsertMidPoints()
    {
        if (spline.pathPoints.Count < 2) return;

        Undo.RecordObject(spline, "Insert Mid Points");

        List<SplinePoint> newPoints = new List<SplinePoint>();

        for (int i = 0; i < spline.pathPoints.Count - 1; i++)
        {
            var a = spline.pathPoints[i];
            var b = spline.pathPoints[i + 1];

            SplinePoint mid = new SplinePoint();
            mid.position = (a.position + b.position) * 0.5f;
            mid.rotation = Quaternion.Slerp(a.rotation, b.rotation, 0.5f);

            newPoints.Add(a);
            newPoints.Add(mid);
        }

        newPoints.Add(spline.pathPoints[spline.pathPoints.Count - 1]);

        spline.pathPoints = newPoints;

        EditorUtility.SetDirty(spline);
    }
}
#endif