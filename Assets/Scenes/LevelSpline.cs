using BeachHero;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelSpline : MonoBehaviour
{
    [Header("Path Points")]
    public List<Vector3> pathPoints = new List<Vector3>();

    [Header("Settings")]
    [Range(5, 100)]
    public int resolution = 20;

    [Header("Target")]
    public Transform target;

    [Range(0f, 1f)]
    public float percent;

    public Vector3 GetTangent(float percent)
    {
        percent = Mathf.Clamp01(percent);

        if (pathPoints == null || pathPoints.Count < 4)
            return Vector3.forward;

        return CatmullSplineUtils.GetTangentOnSpline(pathPoints, percent);
    }

    public Quaternion GetRotation(float percent)
    {
        Vector3 dir = GetTangent(percent);

        if (dir == Vector3.zero)
            return Quaternion.identity;

        return Quaternion.LookRotation(dir, Vector3.up);
    }

    // ---------------------------------------
    // GET POINT ON SPLINE (0 1)
    // ---------------------------------------
    public Vector3 GetPoint(float percent)
    {
        percent = Mathf.Clamp01(percent);

        if (pathPoints == null || pathPoints.Count < 4)
            return transform.position;

        return CatmullSplineUtils.GetPointOnSpline(pathPoints, percent);
    }

    // ---------------------------------------
    // UPDATE TARGET (EDITOR PREVIEW)
    // ---------------------------------------
    private void OnValidate()
    {
        if (target != null && pathPoints != null && pathPoints.Count >= 4)
        {
            target.position = GetPoint(percent);
            target.rotation = GetRotation(percent);
        }
    }
}

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
    // MOVE POINTS (POSITION ONLY)
    // ---------------------------------------
    void DrawPoints()
    {
        for (int i = 0; i < spline.pathPoints.Count; i++)
        {
            Vector3 worldPos = spline.transform.TransformPoint(spline.pathPoints[i]);

            EditorGUI.BeginChangeCheck();

            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");

                spline.pathPoints[i] = spline.transform.InverseTransformPoint(newWorldPos);

                EditorUtility.SetDirty(spline);
            }

            Handles.Label(worldPos + Vector3.up * 0.3f, $"P{i}");
        }
    }

    // ---------------------------------------
    // DRAW CATMULL CURVE
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
                    spline.pathPoints[i],
                    spline.pathPoints[i + 1],
                    spline.pathPoints[i + 2],
                    spline.pathPoints[i + 3],
                    0f
                )
            );

            for (int j = 1; j <= resolution; j++)
            {
                float t = j / (float)resolution;

                Vector3 p = CatmullSplineUtils.GetPoint(
                    spline.pathPoints[i],
                    spline.pathPoints[i + 1],
                    spline.pathPoints[i + 2],
                    spline.pathPoints[i + 3],
                    t
                );

                p = spline.transform.TransformPoint(p);

                Handles.DrawLine(prev, p);
                prev = p;
            }
        }
    }

    // ---------------------------------------
    // ADD POINT
    // ---------------------------------------
    void AddPoint()
    {
        Undo.RecordObject(spline, "Add Point");

        Vector3 newPoint = Vector3.zero;

        if (spline.pathPoints.Count > 0)
        {
            newPoint = spline.pathPoints[spline.pathPoints.Count - 1] + Vector3.forward * 2f;
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

        List<Vector3> newPoints = new List<Vector3>();

        for (int i = 0; i < spline.pathPoints.Count - 1; i++)
        {
            Vector3 a = spline.pathPoints[i];
            Vector3 b = spline.pathPoints[i + 1];

            newPoints.Add(a);
            newPoints.Add((a + b) * 0.5f);
        }

        newPoints.Add(spline.pathPoints[spline.pathPoints.Count - 1]);

        spline.pathPoints = newPoints;

        EditorUtility.SetDirty(spline);
    }
}