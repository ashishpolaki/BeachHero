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

    public LevelVisual levelVisual;
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
    [System.NonSerialized] public int insertIndex = 0;
    [System.NonSerialized] public int removeIndex = 0;
    [System.NonSerialized] public int levelInsertIndex = 0;
    [System.NonSerialized] public int levelRemoveIndex = 0;

    [Header("Levels")]
    public LevelVisual levelPrefab;
    public List<MapLevelSpawnData> mapLevels = new List<MapLevelSpawnData>();
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
    private LevelSpline spline;

    private void OnEnable()
    {
        spline = (LevelSpline)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (spline.editMode == EditMode.Spline)
        {
            GUILayout.Label("Spline Tools", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Point")) AddPoint();
            if (GUILayout.Button("Remove Last")) RemovePoint();
            GUILayout.EndHorizontal();

            //Mid points
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Insert Mid Point", GUILayout.Height(25)))
            {
                InsertMidPoint();
            }
            // Label + Field inline
            GUILayout.Label("Index", GUILayout.Width(40));
            spline.insertIndex = EditorGUILayout.IntField(spline.insertIndex, GUILayout.Width(50));
            // Clamp safely
            spline.insertIndex = Mathf.Clamp(
                spline.insertIndex, 0, Mathf.Max(0, spline.pathPoints.Count - 2));
            GUILayout.EndHorizontal();

            // Remove point
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Remove Point", GUILayout.Height(25)))
            {
                RemovePointAtIndex();
            }

            // Label + Field inline
            GUILayout.Label("Index", GUILayout.Width(40));

            spline.removeIndex = EditorGUILayout.IntField(
                spline.removeIndex,
                GUILayout.Width(50)
            );

            // Clamp safely
            spline.removeIndex = Mathf.Clamp(
                spline.removeIndex,
                0,
                Mathf.Max(0, spline.pathPoints.Count - 1)
            );

            GUILayout.EndHorizontal();

            // Update Debug Objects
            if (GUILayout.Button("Update Debug Objects"))
            {
                spline.UpdateDebugObjects();
            }
        }
        else if (spline.editMode == EditMode.Levels)
        {
            GUILayout.Label("Level Tools", EditorStyles.boldLabel);


            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Level", GUILayout.Height(25)))
            {
                AddLevel();
            }

            if (GUILayout.Button("Remove Last Level", GUILayout.Height(25)))
            {
                RemoveLastLevel();
            }

            GUILayout.EndHorizontal();

            // Add Level at Mid Index
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Level (Mid)", GUILayout.Height(25)))
            {
                AddLevelAtMid();
            }

            GUILayout.Label("Index", GUILayout.Width(40));

            spline.levelInsertIndex = EditorGUILayout.IntField(
                spline.levelInsertIndex,
                GUILayout.Width(50)
            );

            spline.levelInsertIndex = Mathf.Clamp(
                spline.levelInsertIndex,
                0,
                Mathf.Max(0, spline.pathPoints.Count - 2)
            );

            GUILayout.EndHorizontal();

            // Remove Level at Index
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Remove Level", GUILayout.Height(25)))
            {
                RemoveLevelAtIndex();
            }

            GUILayout.Label("Index", GUILayout.Width(40));

            spline.levelRemoveIndex = EditorGUILayout.IntField(
                spline.levelRemoveIndex,
                GUILayout.Width(50)
            );

            spline.levelRemoveIndex = Mathf.Clamp(
                spline.levelRemoveIndex,
                0,
                Mathf.Max(0, spline.mapLevels.Count - 1)
            );

            GUILayout.EndHorizontal();
        }
    }

    private void OnSceneGUI()
    {
        if (spline.pathPoints == null) return;

        if (spline.editMode == EditMode.Spline)
        {
            spline.SetDebugPositionsAndRotations();
            DrawPoints();
            DrawCurve();
        }
        else if (spline.editMode == EditMode.Levels)
        {
        }
    }
    #region Spline Logic
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
            float size = 0.3f / HandleUtility.GetHandleSize(worldPos);
            using (new Handles.DrawingScope(Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one * size)))
            {
                EditorGUI.BeginChangeCheck();

                Quaternion newRot = Handles.RotationHandle(point.rotation, Vector3.zero);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spline, "Rotate Point");

                    point.rotation = newRot;
                    spline.pathPoints[i] = point;

                    spline.UpdateTarget();
                    EditorUtility.SetDirty(spline);
                }
            }
            GUIStyle pointLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            pointLabelStyle.normal.textColor = Color.white;
            pointLabelStyle.fontSize = 22;
            pointLabelStyle.alignment = TextAnchor.MiddleCenter;
            // LABEL
            Vector3 labelPos = worldPos + Vector3.up * HandleUtility.GetHandleSize(worldPos) * 0.3f;
            Handles.Label(labelPos, $"P{i}", pointLabelStyle);

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
            p.position = last.position + Vector3.up * 2;
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

    void RemovePointAtIndex()
    {
        if (spline.pathPoints == null || spline.pathPoints.Count == 0)
            return;

        int index = Mathf.Clamp(
            spline.removeIndex,
            0,
            spline.pathPoints.Count - 1
        );

        Undo.RecordObject(spline, "Remove Point");

        spline.pathPoints.RemoveAt(index);

        EditorUtility.SetDirty(spline);
    }

    void InsertMidPoint()
    {
        if (spline.pathPoints == null || spline.pathPoints.Count < 2)
            return;

        int i = Mathf.Clamp(
            spline.insertIndex,
            0,
            spline.pathPoints.Count - 2
        );

        Undo.RecordObject(spline, "Insert Mid Point");

        var a = spline.pathPoints[i];
        var b = spline.pathPoints[i + 1];

        SplinePoint mid = new SplinePoint()
        {
            position = (a.position + b.position) * 0.5f,
            rotation = Quaternion.Slerp(a.rotation, b.rotation, 0.5f)
        };

        // insert at correct position
        spline.pathPoints.Insert(i + 1, mid);

        EditorUtility.SetDirty(spline);
    }
    #endregion

    #region Levels Logic
    void AddLevelAtMid()
    {
        if (spline.pathPoints.Count < 4) return;

        int i = Mathf.Clamp(
            spline.levelInsertIndex,
            0,
            spline.pathPoints.Count - 2
        );

        Undo.RecordObject(spline, "Add Level Mid");

        float t = 0.5f;
        float percent = (i + t) / (float)(spline.pathPoints.Count - 1);

        Vector3 pos = spline.GetPoint(percent);
        Quaternion rot = spline.GetForwardRotation(percent);

        LevelVisual obj = (LevelVisual)PrefabUtility.InstantiatePrefab(
            spline.levelPrefab,
            spline.levelspawnParent
        );

        obj.transform.position = pos;
        obj.transform.rotation = rot;

        spline.mapLevels.Add(new MapLevelSpawnData()
        {
            levelNumber = spline.mapLevels.Count + 1,
            segmentIndex = i,
            t = t,
            levelVisual = obj
        });

        EditorUtility.SetDirty(spline);
    }
    void RemoveLastLevel()
    {
        if (spline.mapLevels.Count == 0) return;

        Undo.RecordObject(spline, "Remove Last Level");

        var last = spline.mapLevels[^1];

        if (last.levelVisual != null)
        {
            Undo.DestroyObjectImmediate(last.levelVisual.gameObject);
        }

        spline.mapLevels.RemoveAt(spline.mapLevels.Count - 1);

        EditorUtility.SetDirty(spline);
    }
    void RemoveLevelAtIndex()
    {
        if (spline.mapLevels.Count == 0) return;

        int index = Mathf.Clamp(
            spline.levelRemoveIndex,
            0,
            spline.mapLevels.Count - 1
        );

        Undo.RecordObject(spline, "Remove Level");

        var level = spline.mapLevels[index];

        if (level.levelVisual != null)
        {
            Undo.DestroyObjectImmediate(level.levelVisual.gameObject);
        }

        spline.mapLevels.RemoveAt(index);

        EditorUtility.SetDirty(spline);
    }
    void AddLevel()
    {
        if (spline.pathPoints.Count < 4) return;

        Undo.RecordObject(spline, "Add Level");

        float percent = spline.percent;
        Vector3 pos = spline.GetPoint(percent);

        //  Spawn prefab
        LevelVisual levelVisual = (LevelVisual)PrefabUtility.InstantiatePrefab(
            spline.levelPrefab,
            spline.levelspawnParent);

        levelVisual.SetPositions(pos);
        levelVisual.Setup(spline.mapLevels.Count + 1);

        //  Convert percent -> segment + t
        float scaled = percent * (spline.pathPoints.Count - 1);
        int segment = Mathf.FloorToInt(scaled);
        float t = scaled - segment;

        spline.mapLevels.Add(new MapLevelSpawnData()
        {
            levelNumber = spline.mapLevels.Count + 1,
            segmentIndex = segment,
            t = t,
            levelVisual = levelVisual
        });

        EditorUtility.SetDirty(spline);
    }
    #endregion
}
#endif