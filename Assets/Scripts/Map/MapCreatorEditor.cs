#if UNITY_EDITOR
using ES3Types;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomEditor(typeof(MapCreator))]
    public class MapCreatorEditor : Editor
    {
        #region Variables
        public enum EditMode { Spline, Levels }

        // ================= REFERENCES =================
        private MapCreator creator;
        private MapController map;
        private EditMode editMode = EditMode.Spline;

        // ===== TOOL WINDOW =====
        private Rect toolRect = new Rect(800, 20, 250, 220);
        private float splineHeight = 220f;
        private float levelHeight = 140f;
        private float debugExtraHeight = 25f;
        private bool isDragging;
        private Vector2 dragOffset;

        // ===== LEVEL STATE =====
        private LevelVisual previewLevel;
        private int selectedLevelIndex = -1;
        private int levelSegmentIndex = 0;
        private bool isPreviewLevel = false;
        private float levelRotationZ = 0f;
        private float levelScale = 0.5f;
        private float levelT = 0.5f;

        // ===== INDEX FIELDS =====
        private int insertIndex = 1;
        private int removeIndex = 0;

        // ================= DEBUG =================
        private List<Transform> debugObjects = new List<Transform>();
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            creator = (MapCreator)target;

            if (creator.mapController == null)
                creator.mapController = creator.GetComponent<MapController>();

            map = creator.mapController;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            DestroyPreviewLevel();
            ClearDebug();
            Selection.selectionChanged -= OnSelectionChanged;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("Use Scene Window Tool Panel", MessageType.Info);
        }

        private void OnSceneGUI()
        {
            if (map == null) return;
            DrawToolWindow();
            if (editMode == EditMode.Levels)
            {
                UpdatePreview();
                ClearDebug();
            }
            if (editMode == EditMode.Spline)
            {
                DrawCurve();
                DrawPoints();
                DestroyPreviewLevel();
            }
            SceneView.RepaintAll();
        }
        #endregion

        #region Tool Window
        void DrawToolWindow()
        {
            // Adjust height based on mode and debug
            var height = toolRect.height;
            if (editMode == EditMode.Spline)
            {
                height = splineHeight;

                if (creator.showDebug)
                    height += debugExtraHeight;
            }
            else if (editMode == EditMode.Levels)
            {
                height = levelHeight;
            }
            toolRect.height = height;

            // Draw window background
            Handles.BeginGUI();
            EditorGUI.DrawRect(toolRect, new Color(0.18f, 0.18f, 0.18f, 0.95f));
            DrawToolWindowHeader();
            DrawToolWindowContent();
            Handles.EndGUI();
        }

        void DrawToolWindowHeader()
        {
            Rect header = new Rect(toolRect.x, toolRect.y, toolRect.width, 25);

            GUI.Box(header, "Map Tool");

            Event e = Event.current;

            // Dragging
            if (e.type == EventType.MouseDown && header.Contains(e.mousePosition))
            {
                isDragging = true;
                dragOffset = e.mousePosition - toolRect.position;
                e.Use();
            }

            if (e.type == EventType.MouseDrag && isDragging)
            {
                toolRect.position = e.mousePosition - dragOffset;
                e.Use();
            }

            if (e.type == EventType.MouseUp)
                isDragging = false;
        }

        void DrawToolWindowContent()
        {
            GUILayout.BeginArea(new Rect(
                toolRect.x + 10,
                toolRect.y + 30,
                toolRect.width - 20,
                toolRect.height - 40));

            // ===== MODE TOGGLE =====
            GUILayout.BeginHorizontal();

            if (GUILayout.Toggle(editMode == EditMode.Spline, "Spline", GUI.skin.button))
                editMode = EditMode.Spline;

            if (GUILayout.Toggle(editMode == EditMode.Levels, "Levels", GUI.skin.button))
                editMode = EditMode.Levels;

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // ===== CONTENT =====
            if (editMode == EditMode.Spline)
            {
                DrawSplineTools();
            }
            else
            {
                DrawLevelTools();
            }

            GUILayout.EndArea();
        }
        #endregion

        #region Spline Methods
        private void DrawPoints()
        {
            var pts = map.PathPoints;

            for (int i = 0; i < pts.Count; i++)
            {
                var point = pts[i];

                Vector3 worldPos = map.transform.TransformPoint(point.position);

                // POSITION HANDLE
                EditorGUI.BeginChangeCheck();
                Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(map, "Move Point");
                    point.position = map.transform.InverseTransformPoint(newWorldPos);
                    map.UpdateTarget();
                    EditorUtility.SetDirty(map);
                }

                // ROTATION HANDLE
                float size = 0.3f / HandleUtility.GetHandleSize(worldPos);
                using (new Handles.DrawingScope(Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one * size)))
                {
                    EditorGUI.BeginChangeCheck();

                    Quaternion newRot = Handles.RotationHandle(point.rotation, Vector3.zero);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(map, "Rotate Point");

                        point.rotation = newRot;
                        map.PathPoints[i] = point;

                        map.UpdateTarget();
                        EditorUtility.SetDirty(map);
                    }
                }
                GUIStyle pointLabelStyle = new GUIStyle(EditorStyles.boldLabel);
                pointLabelStyle.normal.textColor = Color.white;
                pointLabelStyle.fontSize = 22;
                pointLabelStyle.alignment = TextAnchor.MiddleCenter;
                // LABEL
                Vector3 labelPos = worldPos + Vector3.up * HandleUtility.GetHandleSize(worldPos) * 0.3f;
                Handles.Label(labelPos, $"P{i}", pointLabelStyle);
            }
        }

        void DrawCurve()
        {
            var pts = map.PathPoints;

            if (pts.Count < 4) return;

            Handles.color = Color.green;

            for (int i = 0; i < pts.Count - 3; i++)
            {
                Vector3 prev = map.transform.TransformPoint(
                    CatmullSplineUtils.GetPoint(
                        pts[i].position,
                        pts[i + 1].position,
                        pts[i + 2].position,
                        pts[i + 3].position,
                        0f
                    ));

                for (int j = 1; j <= map.resolution; j++)
                {
                    float t = j / (float)map.resolution;

                    Vector3 p = map.transform.TransformPoint(
                        CatmullSplineUtils.GetPoint(
                            pts[i].position,
                            pts[i + 1].position,
                            pts[i + 2].position,
                            pts[i + 3].position,
                            t
                        ));

                    Handles.DrawLine(prev, p);
                    prev = p;
                }
            }
        }

        void DrawSplineTools()
        {
            var pts = map.PathPoints;

            // ===== ADD POINT =====
            if (GUILayout.Button("Add Point"))
                AddPoint();

            // ===== INSERT INDEX =====
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Insert Mid At Index"))
                InsertMidPointAtIndex();
            insertIndex = EditorGUILayout.IntField(insertIndex, GUILayout.Width(50));
            insertIndex = Mathf.Clamp(insertIndex, 1, Mathf.Max(1, pts.Count - 1));
            GUILayout.EndHorizontal();

            // ===== REMOVE INDEX =====
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove At Index"))
                RemovePointAtIndex();
            removeIndex = EditorGUILayout.IntField(removeIndex, GUILayout.Width(50));
            removeIndex = Mathf.Clamp(removeIndex, 0, Mathf.Max(0, pts.Count - 1));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            // ===== PERCENT (WITH CHANGE CHECK) =====
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 80;
            float newPercent = EditorGUILayout.Slider("Percent", map.percent, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(map, "Change Percent");
                map.percent = newPercent;
                map.UpdateTarget();
                EditorUtility.SetDirty(map);
            }

            // ===== RESOLUTION =====
            EditorGUIUtility.labelWidth = 80;
            map.resolution = EditorGUILayout.IntSlider("Resolution", map.resolution, 5, 100);
            EditorGUIUtility.labelWidth = 0;

            DrawDebugTools();

            // mark dirty so it updates in editor
            if (GUI.changed)
            {
                EditorUtility.SetDirty(map);
                EditorUtility.SetDirty(creator);
            }
        }

        void AddPoint()
        {
            Undo.RecordObject(map, "Add Point");
            SplinePoint p = new SplinePoint();

            if (map.PathPoints.Count > 0)
            {
                var last = map.PathPoints[^1];
                p.position = last.position + Vector3.up * 2;
                p.rotation = last.rotation;
            }

            map.PathPoints.Add(p);
            EditorUtility.SetDirty(map);
        }
        void InsertMidPointAtIndex()
        {
            var pts = map.PathPoints;

            if (pts.Count < 2) return;
            if (insertIndex <= 0 || insertIndex >= pts.Count) return;

            Undo.RecordObject(map, "Insert Mid Point");

            Vector3 a = pts[insertIndex - 1].position;
            Vector3 b = pts[insertIndex].position;

            Vector3 mid = (a + b) * 0.5f;

            pts.Insert(insertIndex, new SplinePoint() { position = mid });

            EditorUtility.SetDirty(map);
        }

        void RemovePointAtIndex()
        {
            var pts = map.PathPoints;

            if (pts.Count <= 4) return;
            if (removeIndex < 0 || removeIndex >= pts.Count) return;

            Undo.RecordObject(map, "Remove Point");

            pts.RemoveAt(removeIndex);

            EditorUtility.SetDirty(map);
        }

        #endregion

        #region Levels Methods

        void DrawLevelTools()
        {
            if (GUILayout.Button("Add Level"))
            {
                // popupSegmentIndex = 0;
                // popupT = 0.5f;
                // CreatePreview();
            }

            if (GUILayout.Button("Insert Level"))
            {
                Debug.Log("Insert Level (next step)");
            }

            if (GUILayout.Button("Remove Level"))
            {
                Debug.Log("Remove Level (next step)");
            }
        }
        private List<MapLevelSpawnData> GetLevels()
        {
            var field = typeof(MapController)
                .GetField("mapLevels", BindingFlags.NonPublic | BindingFlags.Instance);

            return (List<MapLevelSpawnData>)field.GetValue(map);
        }

        #region Preview Level
        void CreatePreviewLevel()
        {
            if (previewLevel != null) return;

            if (creator.levelPrefab == null)
            {
                //Helpbox
                EditorGUI.HelpBox(new Rect(10, 70, 300, 40), "Assign Level Prefab in MapCreator", MessageType.Error);
                return;
            }

            previewLevel = (LevelVisual)PrefabUtility.InstantiatePrefab(
                creator.levelPrefab,
                map.LevelsParent
            );

            previewLevel.name = "PREVIEW_Level";
            previewLevel.Setup(selectedLevelIndex + 1, levelScale);
            // Optional: make it visually distinct
            previewLevel.gameObject.hideFlags = HideFlags.DontSave;
            isPreviewLevel = true;
        }

        void UpdatePreview()
        {
            if (previewLevel == null) return;

            int count = map.GetPositions().Count;
            if (count < 2) return;

            float percent = (levelSegmentIndex + levelT) / (count - 1);

            previewLevel.transform.position = map.GetPoint(percent);
            previewLevel.transform.rotation = Quaternion.Euler(0, 0, levelRotationZ);
            previewLevel.Setup(selectedLevelIndex + 1, levelScale);
        }

        void DestroyPreviewLevel()
        {
            if (previewLevel != null && isPreviewLevel)
                DestroyImmediate(previewLevel.gameObject);

            previewLevel = null;
            isPreviewLevel = false;
        }

        void ConfirmLevel()
        {
            if (previewLevel == null) return;

            var levels = GetLevels();

            if (selectedLevelIndex >= 0 && selectedLevelIndex < levels.Count)
            {
                var data = levels[selectedLevelIndex];

                data.segmentIndex = levelSegmentIndex;
                data.t = levelT;
                data.rotation = new Vector3(0, 0, levelRotationZ);
                data.scale = Vector3.one * levelScale;

                levels[selectedLevelIndex] = data;
            }
            else
            {
                levels.Add(new MapLevelSpawnData()
                {
                    levelNumber = selectedLevelIndex + 1,
                    segmentIndex = levelSegmentIndex,
                    t = levelT,
                    rotation = new Vector3(0, 0, levelRotationZ),
                    scale = Vector3.one * levelScale,
                    levelVisual = previewLevel
                });
            }

            previewLevel = null;
            selectedLevelIndex = -1;
            EditorUtility.SetDirty(map);
        }
        #endregion

        #endregion

        #region Selection
        private void OnSelectionChanged()
        {
            if (Selection.activeGameObject == null ||
                Selection.activeGameObject != map.gameObject)
            {
                DestroyPreviewLevel();
                SceneView.RepaintAll();
            }
        }
        void HandleSelection()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                RaycastHit2D hit = Physics2D.Raycast(
                    ray.origin,
                    ray.direction,
                    Mathf.Infinity,
                    LayerMask.GetMask("Map")
                );

                if (hit.collider != null)
                {
                    LevelVisual lv = hit.collider.GetComponentInParent<LevelVisual>();

                    if (lv != null)
                    {
                        SelectLevel(lv);

                        EditorApplication.delayCall += () =>
                        {
                            Selection.activeGameObject = map.gameObject;
                        };

                        e.Use();
                    }
                }
            }
        }

        void SelectLevel(LevelVisual level)
        {
            var levels = GetLevels();

            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].levelVisual == level)
                {
                    selectedLevelIndex = i;

                    var data = levels[i];

                    levelSegmentIndex = data.segmentIndex;
                    levelT = data.t;
                    levelRotationZ = data.rotation.z;
                    levelScale = data.scale.x;

                    previewLevel = level;
                    isPreviewLevel = false;

                    return;
                }
            }
        }
        #endregion

        #region Debug Methods
        private void DrawDebugTools()
        {
            // ================= DEBUG SECTION =================
            GUILayout.Space(5);
            GUILayout.Label("Debug", EditorStyles.boldLabel);

            creator.showDebug = EditorGUILayout.Toggle("Show Debug", creator.showDebug);

            if (creator.showDebug)
            {
                EditorGUIUtility.labelWidth = 80;
                creator.debugCount = EditorGUILayout.IntSlider("Count", creator.debugCount, 2, 100);

                //  Update debug in real-time
                ShowDebug();
            }
            else
            {
                ClearDebug();
            }
        }
        void ShowDebug()
        {
            if (creator.debugPrefab == null) return;

            int count = creator.debugCount;
            int points = map.GetPositions().Count;

            if (points < 4) return;

            while (debugObjects.Count < count)
            {
                var obj = (Transform)PrefabUtility.InstantiatePrefab(
                    creator.debugPrefab,
                    creator.debugParent
                );
                debugObjects.Add(obj);
            }

            while (debugObjects.Count > count)
            {
                DestroyImmediate(debugObjects[^1].gameObject);
                debugObjects.RemoveAt(debugObjects.Count - 1);
            }

            for (int i = 0; i < count; i++)
            {
                float percent = i / (float)(count - 1);

                var obj = debugObjects[i];

                obj.position = map.GetPoint(percent);
                obj.rotation = map.GetForwardRotation(percent);

                if (obj.childCount > 0)
                    obj.GetChild(0).localRotation = GetTwistRotation(percent);
            }
        }
        void ClearDebug()
        {
            foreach (var obj in debugObjects)
            {
                if (obj != null)
                    DestroyImmediate(obj.gameObject);
            }

            debugObjects.Clear();
        }
        private Quaternion GetTwistRotation(float percent)
        {
            percent = Mathf.Clamp01(percent);

            int count = map.PathPoints.Count;
            if (count < 2)
                return Quaternion.identity;

            float scaled = percent * (count - 1);
            int i = Mathf.FloorToInt(scaled);

            i = Mathf.Clamp(i, 0, count - 2);

            return map.PathPoints[i].rotation;
        }
        #endregion
    }
}
#endif
