using BeachHero;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class MapLevelSpawnData
{
    public int levelNumber;

    [Header("Spline Position")]
    public int segmentIndex;

    [Range(0f, 1f)]
    public float t;

    [Header("Transform")]
    public Vector3 scale = Vector3.one;
    public Vector3 rotation;
}
[System.Serializable]
public class SplinePoint
{
    public Vector3 position;
    public Quaternion rotation = Quaternion.identity;
}
public enum EditMode
{
    Spline,
    Levels
}
public class LevelSpline : MonoBehaviour
{
    [Header("Edit Mode")]
    public EditMode editMode;

    [Header("Path Points")]
    public List<SplinePoint> pathPoints = new List<SplinePoint>();

    [Header("Levels")]
    public List<MapLevelSpawnData> mapLevels = new List<MapLevelSpawnData>();
    public GameObject levelPrefab;
    public Transform levelspawnParent;

    [Header("Settings")]
    [Range(5, 100)]
    public int resolution = 20;

    [Header("Preview")]
    public Transform target;
    public Transform visualChild;
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

        //if (Quaternion.Dot(a, b) < 0f)
        //{
        //    b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
        //}

        Quaternion rot = Quaternion.Slerp(a, b, t);
        return rot;
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
            {
                percent = Mathf.Clamp01(percent);

                int count = pathPoints.Count;
                Quaternion rot = Quaternion.identity;

                float scaled = percent * (count - 1);
                int i = Mathf.FloorToInt(scaled);
                float t = scaled - i;

                i = Mathf.Clamp(i, 0, count - 2);

                Quaternion a = pathPoints[i].rotation;
                Quaternion b = pathPoints[i + 1].rotation;

                //  CRITICAL FIX (prevents flip)
                if (Quaternion.Dot(a, b) < 0f)
                {
                    b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
                }

                rot = Quaternion.Slerp(a, b, t);
                rot = Quaternion.Normalize(rot);
                DebugUtils.LogWarning($"Rot  {rot}");
                visualChild.localRotation = rot;
            }
        }
    }


    [Header("Debug")]
    public bool showDebugObjects = true;
    [Range(2, 100)]
    public int debugCount = 20;
    public Transform debugPrefab;

    public List<Transform> debugObjects = new List<Transform>();

    private void CreateDebugObjects()
    {
        if (debugObjects == null)
            debugObjects = new List<Transform>();

        // Create only if needed
        while (debugObjects.Count < debugCount)
        {
            var obj = Instantiate(debugPrefab, transform);
            debugObjects.Add(obj);
        }

        // Remove extras
        while (debugObjects.Count > debugCount)
        {
            var last = debugObjects[^1];

            if (last != null)
            {
                DestroyImmediate(last.gameObject);
            }
            debugObjects.RemoveAt(debugObjects.Count - 1);
        }

        //  Ensure all active
        for (int i = 0; i < debugObjects.Count; i++)
        {
            debugObjects[i].gameObject.SetActive(true);
        }
    }
    public void UpdateDebugObjects()
    {
        if (!showDebugObjects || debugPrefab == null || pathPoints.Count < 4)
        {
            return;
        }
        ClearDebugObjects();
        CreateDebugObjects();
        SetDebugPositionsAndRotations();
    }
    public void SetDebugPositionsAndRotations()
    {
        if (debugObjects == null || debugObjects.Count == 0)
            return;

        for (int i = 0; i < debugCount; i++)
        {
            float percent = i / (float)(debugCount - 1);

            Vector3 pos = GetPoint(percent);
            Quaternion rot = GetForwardRotation(percent);

            debugObjects[i].position = pos;
            debugObjects[i].rotation = rot;
            debugObjects[i].GetChild(0).localRotation = GetTwistRotation(percent);
        }
    }
    public void ClearDebugObjects()
    {
        for (int i = 0; i < debugObjects.Count; i++)
        {
            if (debugObjects[i] != null)
            {
                DestroyImmediate(debugObjects[i].gameObject);
            }
        }
        debugObjects.Clear();
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

        if (GUILayout.Button("Update Debug Objects"))
        {
            spline.UpdateDebugObjects();
        }
    }

    private void OnSceneGUI()
    {
        if (spline.pathPoints == null) return;
        spline.SetDebugPositionsAndRotations();
        spline.OnSpriteUpdate();
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
    }

    // ---------------------------------------
    // ADD POINT
    // ---------------------------------------
    void AddPoint()
    {
        Undo.RecordObject(spline, "Add Point");

        SplinePoint p = new SplinePoint();

        if (spline.pathPoints.Count > 0)
        {
            var last = spline.pathPoints[^1];
            p.position = last.position + Vector3.forward * 2;
            p.rotation = last.rotation;
        }

        spline.pathPoints.Add(p);

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

    void InsertMidPoints()
    {
        Undo.RecordObject(spline, "Insert Mid");

        var list = new List<SplinePoint>();
        List<SplinePoint> newPoints = new List<SplinePoint>();
        for (int i = 0; i < spline.pathPoints.Count - 1; i++)
        {
            var a = spline.pathPoints[i];
            var b = spline.pathPoints[i + 1];

            list.Add(a);

            list.Add(new SplinePoint()
            {
                position = (a.position + b.position) * 0.5f,
                rotation = Quaternion.Slerp(a.rotation, b.rotation, 0.5f)
            });
        }

        newPoints.Add(spline.pathPoints[spline.pathPoints.Count - 1]);
        list.Add(spline.pathPoints[^1]);

        spline.pathPoints = newPoints;
        spline.pathPoints = list;

        EditorUtility.SetDirty(spline);
    }
    // -------------------------
    // LEVEL MODE
    // -------------------------
    void DrawLevelButtons()
    {
        if (GUILayout.Button("Add Level"))
        {
            Undo.RecordObject(spline, "Add Level");

            spline.mapLevels.Add(new MapLevelSpawnData()
            {
                levelNumber = spline.mapLevels.Count + 1,
                segmentIndex = 0,
                t = 0.5f
            });

            EditorUtility.SetDirty(spline);
        }

        if (GUILayout.Button("Spawn Map"))
        {
            // spline.SpawnMap();
        }
    }

    void DrawLevels()
    {
        for (int i = 0; i < spline.mapLevels.Count; i++)
        {
            var level = spline.mapLevels[i];

            //Vector3 pos = spline.transform.TransformPoint(
            //    spline.GetPointOnSegment(level.segmentIndex, level.t)
            //);

            //Handles.color = Color.cyan;
            //Handles.SphereHandleCap(0, pos, Quaternion.identity, 0.4f, EventType.Repaint);

            //Handles.Label(pos + Vector3.up * 0.5f,
            //    $"Level {level.levelNumber}\nSeg:{level.segmentIndex} t:{level.t:F2}");
        }
    }
}
#endif