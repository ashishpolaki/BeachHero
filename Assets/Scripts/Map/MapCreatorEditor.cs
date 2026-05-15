#if UNITY_EDITOR
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
        private EditMode editMode = EditMode.Levels;

        // ===== TOOL WINDOW =====
        private Rect toolRect = new Rect(800, 20, 250, 220);
        private float splineHeight = 220f;
        private float levelHeight = 140f;
        private float debugExtraHeight = 25f;
        private float addLevelExtraHeight = 150f;
        private bool isDragging;
        private Vector2 dragOffset;

        // ===== LEVEL STATE =====
        private LevelVisual previewLevel;
        private int selectedLevelIndex = -1;
        private int levelSegmentIndex = 0;
        private int insertLevelNumber = 1;
        private int removeLevelNumber = 1;
        private float levelRotationZ = 0f;
        private float levelScale = 0.5f;
        private float levelT = 0.5f;
        private bool isPreviewLevel = false;
        private bool showAddLevelUI = false;

        // ===== SPLINE FIELDS =====
        private int insertPointIndex = 1;
        private int removePointIndex = 0;
        private float splinePercent = 0f;

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
            ReorderLevels();
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
            DrawCurve();
            if (editMode == EditMode.Levels)
            {
                HandleLevelSelection();
                UpdatePreviewLevel();
                ClearDebug();
            }
            if (editMode == EditMode.Spline)
            {
                showAddLevelUI = false;
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

                if (showAddLevelUI)
                    height += addLevelExtraHeight;
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
                    map.UpdateCharacterTransform(splinePercent);
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

                        map.UpdateCharacterTransform(splinePercent);
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

            Handles.color = Color.yellow;

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
            insertPointIndex = EditorGUILayout.IntField(insertPointIndex, GUILayout.Width(50));
            insertPointIndex = Mathf.Clamp(insertPointIndex, 1, Mathf.Max(1, pts.Count - 1));
            GUILayout.EndHorizontal();

            // ===== REMOVE INDEX =====
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove At Index"))
                RemovePointAtIndex();
            removePointIndex = EditorGUILayout.IntField(removePointIndex, GUILayout.Width(50));
            removePointIndex = Mathf.Clamp(removePointIndex, 0, Mathf.Max(0, pts.Count - 1));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            // ===== PERCENT (WITH CHANGE CHECK) =====
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 80;
            splinePercent = EditorGUILayout.Slider("Percent", splinePercent, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(map, "Change Percent");
                map.UpdateCharacterTransform(splinePercent);
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
            if (insertPointIndex <= 0 || insertPointIndex >= pts.Count) return;

            Undo.RecordObject(map, "Insert Mid Point");

            Vector3 a = pts[insertPointIndex - 1].position;
            Vector3 b = pts[insertPointIndex].position;

            Vector3 mid = (a + b) * 0.5f;

            pts.Insert(insertPointIndex, new SplinePoint() { position = mid });

            EditorUtility.SetDirty(map);
        }

        void RemovePointAtIndex()
        {
            var pts = map.PathPoints;

            if (pts.Count <= 4) return;
            if (removePointIndex < 0 || removePointIndex >= pts.Count) return;

            Undo.RecordObject(map, "Remove Point");

            pts.RemoveAt(removePointIndex);

            EditorUtility.SetDirty(map);
        }

        #endregion

        #region Levels Methods

        void DrawLevelTools()
        {
            var levels = GetLevels();
            int count = levels.Count;

            // ===== NORMAL MODE =====
            if (!showAddLevelUI)
            {
                // ===== ADD LEVEL =====
                if (GUILayout.Button("Add Level"))
                {
                    showAddLevelUI = true;

                    selectedLevelIndex = GetLevels().Count;
                    // Set default segment and t based on last level
                    if (levels.Count > 0)
                    {
                        var last = levels[^1];

                        levelSegmentIndex = last.segmentIndex;
                        levelT = last.t + 0.5f;

                        if (levelT > 1f)
                        {
                            levelSegmentIndex += 1;
                            levelT = 0.5f;
                        }

                        int maxSegment = Mathf.Max(0, map.PathPoints.Count - 2);
                        levelSegmentIndex = Mathf.Clamp(levelSegmentIndex, 0, maxSegment);
                    }
                    // If no levels, start at beginning
                    else
                    {
                        levelSegmentIndex = 0;
                        levelT = 0f;
                    }
                    levelRotationZ = 0f;
                    levelScale = 0.5f;

                    CreatePreviewLevel();
                }

                // ===== INSERT LEVEL =====
                GUILayout.BeginHorizontal();
                GUI.enabled = count > 1;
                if (GUILayout.Button("Insert Level", GUILayout.Width(120)))
                {
                    InsertLevelAtIndex();
                }
                insertLevelNumber = Mathf.RoundToInt(GUILayout.HorizontalSlider(insertLevelNumber, 1, Mathf.Max(1, count - 1)));
                insertLevelNumber = EditorGUILayout.IntField(insertLevelNumber, GUILayout.Width(40));
                insertLevelNumber = Mathf.Clamp(insertLevelNumber, 1, Mathf.Max(1, count - 1));//  CLAMP 
                GUILayout.EndHorizontal();
                GUI.enabled = true;

                // ===== REMOVE LEVEL =====
                GUI.enabled = count > 0;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove Level", GUILayout.Width(120)))
                {
                    RemoveLevelAtIndex();
                }
                removeLevelNumber = Mathf.RoundToInt(GUILayout.HorizontalSlider(removeLevelNumber, 1, Mathf.Max(0, count)));
                removeLevelNumber = EditorGUILayout.IntField(removeLevelNumber, GUILayout.Width(40));
                removeLevelNumber = Mathf.Clamp(removeLevelNumber, 0, Mathf.Max(0, count));// CLAMP
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }
            else
            {
                DrawAddLevelUI();
            }
        }
        private List<MapLevelSpawnData> GetLevels()
        {
            var field = typeof(MapController)
                .GetField("mapLevels", BindingFlags.NonPublic | BindingFlags.Instance);

            return (List<MapLevelSpawnData>)field.GetValue(map);
        }
        void InsertLevelAtIndex()
        {
            var levels = GetLevels();

            if (creator.levelPrefab == null) return;
            int index = insertLevelNumber - 1;
            var newLevel = (LevelVisual)PrefabUtility.InstantiatePrefab(
                creator.levelPrefab,
                map.LevelsParent
            );

            newLevel.Setup(insertLevelNumber + 1, levelScale);

            // Determine segment and t based on neighbors
            var newSegment = levelSegmentIndex;
            var newT = levelT;
            if (index > 0 && index < levels.Count)
            {
                var prev = levels[index - 1];
                var next = levels[index];

                if (prev.segmentIndex == next.segmentIndex)
                {
                    //same segment -> interpolate t
                    newSegment = prev.segmentIndex;
                    newT = (prev.t + next.t) * 0.5f;
                }
                else
                {
                    // different segments -> choose closer one
                    newSegment = prev.segmentIndex;
                    newT = 1f; // end of previous segment
                }
            }

            var data = new MapLevelSpawnData()
            {
                levelNumber = insertLevelNumber,
                segmentIndex = newSegment,
                t = newT,
                rotation = new Vector3(0, 0, levelRotationZ),
                scale = Vector3.one * levelScale,
                levelVisual = newLevel
            };

            levels.Insert(index, data);

            // Fix numbering
            ReorderLevels();

            selectedLevelIndex = index;
            previewLevel = newLevel;
            isPreviewLevel = false;

            EditorApplication.delayCall += () =>
            {
                if (map != null)
                    Selection.activeGameObject = map.gameObject;
            };
            EditorUtility.SetDirty(map);
            PrefabUtility.RecordPrefabInstancePropertyModifications(newLevel.gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(map);

            PrefabUtility.ApplyPrefabInstance(
                map.gameObject,
                InteractionMode.UserAction
            );
        }

        void RemoveLevelAtIndex()
        {
            var levels = GetLevels();
            if (levels.Count == 0) return;
            int index = removeLevelNumber - 1;
            if (index < 0 || index >= levels.Count) return;

            var data = levels[index];

            if (data.levelVisual != null)
                DestroyImmediate(data.levelVisual.gameObject);

            levels.RemoveAt(index);

            //  Fix numbering
            ReorderLevels();

            EditorUtility.SetDirty(map);
            PrefabUtility.RecordPrefabInstancePropertyModifications(map);
            PrefabUtility.ApplyPrefabInstance(
                map.gameObject,
                InteractionMode.UserAction
            );
        }

        void ReorderLevels()
        {
            var levels = GetLevels();

            for (int i = 0; i < levels.Count; i++)
            {
                levels[i].levelNumber = i + 1;

                if (levels[i].levelVisual != null)
                {
                    levels[i].levelVisual.name = $"Level_{i + 1}";
                    levels[i].levelVisual.transform.position = map.GetPoint((levels[i].segmentIndex + levels[i].t) / (map.GetPositions().Count - 1));
                    levels[i].levelVisual.transform.rotation = Quaternion.Euler(levels[i].rotation);
                    levels[i].splinePercent = (levels[i].segmentIndex + levels[i].t) / (map.GetPositions().Count - 1);
                    levels[i].levelVisual.Setup(i + 1, levels[i].scale.x);
                }
            }
        }
        void DrawAddLevelUI()
        {
            var pts = map.PathPoints;

            EditorGUILayout.LabelField("Level Number", (selectedLevelIndex + 1).ToString());

            GUILayout.Space(5);

            // ===== SEGMENT =====
            GUILayout.Label("Segment Index");
            levelSegmentIndex = EditorGUILayout.IntSlider(
                levelSegmentIndex,
                0,
                Mathf.Max(0, pts.Count - 2)
            );

            // ===== PERCENT =====
            GUILayout.Label("Percentage");
            levelT = EditorGUILayout.Slider(levelT, 0f, 1f);

            GUILayout.Space(5);

            // ===== ROTATION =====
            GUILayout.Label("Rotation (Z)");
            levelRotationZ = EditorGUILayout.Slider(levelRotationZ, 0f, 360f);

            // ===== SCALE =====
            GUILayout.Label("Scale");
            levelScale = EditorGUILayout.Slider(levelScale, 0.1f, 0.7f);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("OK"))
            {
                ConfirmLevel();
                showAddLevelUI = false;
            }

            if (GUILayout.Button("Cancel"))
            {
                DestroyPreviewLevel();
                ReorderLevels();
                showAddLevelUI = false;
            }

            GUILayout.EndHorizontal();
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
            Undo.RegisterCreatedObjectUndo(previewLevel, "Create Level");
            previewLevel.name = "PREVIEW_Level";
            previewLevel.Setup(selectedLevelIndex + 1, levelScale);
            // Optional: make it visually distinct
            previewLevel.gameObject.hideFlags = HideFlags.DontSave;
            isPreviewLevel = true;
        }

        void UpdatePreviewLevel()
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
            previewLevel.gameObject.hideFlags = HideFlags.None;

            if (selectedLevelIndex >= 0 && selectedLevelIndex < levels.Count)
            {
                var data = levels[selectedLevelIndex];

                data.segmentIndex = levelSegmentIndex;
                data.t = levelT;
                data.rotation = new Vector3(0, 0, levelRotationZ);
                data.scale = Vector3.one * levelScale;
                data.splinePercent = (data.segmentIndex + data.t) / (map.GetPositions().Count - 1);

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
            previewLevel.Setup(selectedLevelIndex + 1, levelScale);
            previewLevel.name = $"Level_{selectedLevelIndex + 1}";
            //  Record changes
            PrefabUtility.RecordPrefabInstancePropertyModifications(previewLevel.gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(map);

            // APPLY to prefab
            PrefabUtility.ApplyPrefabInstance(
                map.gameObject,
                InteractionMode.UserAction
            );

            previewLevel = null;
            selectedLevelIndex = -1;
            EditorUtility.SetDirty(map);
        }
        #endregion

        #endregion

        #region Level Selection
        private void OnSelectionChanged()
        {
            if (map == null) return;
            if (Selection.activeGameObject == null ||
                Selection.activeGameObject != map.gameObject)
            {
                DestroyPreviewLevel();
                SceneView.RepaintAll();
            }
        }
        private void HandleLevelSelection()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Map")))
                {
                    LevelVisual lv = hitInfo.collider.GetComponentInParent<LevelVisual>();
                    if (lv != null)
                    {
                        SelectLevel(lv);
                        EditorApplication.delayCall += () =>
                        {
                            Selection.activeGameObject = creator.gameObject;
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
                    selectedLevelIndex = level.LevelNumber - 1;
                    var data = levels[i];
                    levelSegmentIndex = data.segmentIndex;
                    levelT = data.t;
                    levelRotationZ = data.rotation.z;
                    levelScale = data.scale.x;
                    previewLevel = level;
                    isPreviewLevel = false;
                    showAddLevelUI = true;

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
