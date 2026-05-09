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
        public enum EditMode
        {
            Spline,
            Levels
        }

        // ================= REFERENCES =================
        private MapCreator creator;
        private MapController map;
        private EditMode editMode = EditMode.Levels;

        // ===== LEVEL STATE =====
        private LevelVisual previewLevel;
        private int selectedLevelIndex = -1;
        private bool isPreviewLevel = false;

        // ===== POPUP =====
        private bool showPopup = false;
        private Rect popupRect = new Rect(0, 0, 260, 260);
        private bool initPopupPos = true;

        private float popupRotationZ = 0f;
        private float popupScale = 0.5f;
        private int popupSegmentIndex = 0;
        private float popupT = 0.5f;

        // ===== DRAG =====
        private bool isDragging = false;
        private Vector2 dragOffset;

        // ================= DEBUG =================
        private List<Transform> debugObjects = new List<Transform>();

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
            Selection.selectionChanged -= OnSelectionChanged;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (map == null)
            {
                EditorGUILayout.HelpBox("Assign MapController", MessageType.Warning);
                return;
            }

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Spline Mode"))
                editMode = EditMode.Spline;

            if (GUILayout.Button("Level Mode"))
                editMode = EditMode.Levels;

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // ===== ADD LEVEL =====
            if (editMode == EditMode.Levels)
            {
                if (GUILayout.Button("Add Level"))
                {
                    showPopup = true;
                    initPopupPos = true;

                    selectedLevelIndex = map.MapLevels.Count;

                    popupSegmentIndex = 0;
                    popupT = 0.5f;
                    popupRotationZ = 0f;
                    popupScale = 0.5f;

                    CreatePreviewLevel();
                }
            }
            else if (editMode == EditMode.Spline)
            {

            }
            GUILayout.Space(10);
        }

        private void OnSceneGUI()
        {
            if (map == null) return;

            if (editMode == EditMode.Levels)
            {
                HandleSelection();
                DrawPopup();
                UpdatePreview();
            }else if (editMode == EditMode.Spline)
            {
                DrawCurve();
                DrawPoints();
                DestroyPreviewLevel();
            }

            SceneView.RepaintAll();
        }
        #endregion

        #region Spline Methods
        void DrawPoints()
        {
            var pts = map.PathPoints;

            for (int i = 0; i < pts.Count; i++)
            {
                Vector3 worldPos = map.transform.TransformPoint(pts[i].position);

                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(worldPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(map, "Move Point");
                    pts[i].position = map.transform.InverseTransformPoint(newPos);
                    EditorUtility.SetDirty(map);
                }

                float size = 0.3f / HandleUtility.GetHandleSize(worldPos);

                using (new Handles.DrawingScope(Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one * size)))
                {
                    EditorGUI.BeginChangeCheck();

                    Quaternion newRot = Handles.RotationHandle(pts[i].rotation, Vector3.zero);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(map, "Rotate Point");
                        pts[i].rotation = newRot;
                        EditorUtility.SetDirty(map);
                    }
                }

                Handles.Label(worldPos, $"P{i}");
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
        #endregion

        #region Levels Methods

        #endregion

        private List<MapLevelSpawnData> GetLevels()
        {
            var field = typeof(MapController)
                .GetField("mapLevels", BindingFlags.NonPublic | BindingFlags.Instance);

            return (List<MapLevelSpawnData>)field.GetValue(map);
        }

        private void OnSelectionChanged()
        {
            if (Selection.activeGameObject == null ||
                Selection.activeGameObject != map.gameObject)
            {
                DestroyPreviewLevel();
                showPopup = false;
                SceneView.RepaintAll();
            }
        }

        #region Selection
        void HandleSelection()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && !showPopup)
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

                    popupSegmentIndex = data.segmentIndex;
                    popupT = data.t;
                    popupRotationZ = data.rotation.z;
                    popupScale = data.scale.x;

                    previewLevel = level;
                    isPreviewLevel = false;

                    showPopup = true;
                    initPopupPos = true;

                    return;
                }
            }
        }
        #endregion

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
            previewLevel.Setup(selectedLevelIndex + 1, popupScale);
            // Optional: make it visually distinct
            previewLevel.gameObject.hideFlags = HideFlags.DontSave;
            isPreviewLevel = true;
        }

        void UpdatePreview()
        {
            if (!showPopup || previewLevel == null) return;

            int count = map.GetPositions().Count;
            if (count < 2) return;

            float percent = (popupSegmentIndex + popupT) / (count - 1);

            previewLevel.transform.position = map.GetPoint(percent);
            previewLevel.transform.rotation = Quaternion.Euler(0, 0, popupRotationZ);
            previewLevel.Setup(selectedLevelIndex + 1, popupScale);
        }

        void DestroyPreviewLevel()
        {
            if (previewLevel != null && isPreviewLevel)
                DestroyImmediate(previewLevel.gameObject);

            previewLevel = null;
            isPreviewLevel = false;
        }
        #endregion

        #region Popup
        void DrawPopup()
        {
            if (!showPopup) return;

            SceneView view = SceneView.currentDrawingSceneView;
            if (view == null) return;

            if (initPopupPos)
            {
                popupRect.x = (view.position.width - popupRect.width) / 2;
                popupRect.y = 40;
                initPopupPos = false;
            }

            Handles.BeginGUI();

            EditorGUI.DrawRect(popupRect, new Color(0.18f, 0.18f, 0.18f, 0.95f));

            DrawPopupHeader();
            DrawPopupContent();

            Handles.EndGUI();
        }

        private void DrawPopupHeader()
        {
            Rect header = new Rect(popupRect.x, popupRect.y, popupRect.width, 25);

            GUI.Box(header, "Level Editor");

            Event e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (header.Contains(e.mousePosition))
                    {
                        isDragging = true;
                        dragOffset = e.mousePosition - new Vector2(popupRect.x, popupRect.y);
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (isDragging)
                    {
                        popupRect.position = e.mousePosition - dragOffset;
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    isDragging = false;
                    break;
            }
        }

        void DrawPopupContent()
        {
            GUILayout.BeginArea(new Rect(
            popupRect.x + 10,
            popupRect.y + 30,
            popupRect.width - 20,
            popupRect.height - 40));

            // LEVEL NUMBER (READ ONLY)
            EditorGUILayout.LabelField("Level Number", (selectedLevelIndex + 1).ToString());
            GUILayout.Space(5);

            // SEGMENT
            GUILayout.Label("Segment Index");
            popupSegmentIndex = EditorGUILayout.IntSlider(
                popupSegmentIndex,
                0,
                Mathf.Max(0, map.PathPoints.Count - 2)
            );

            // PERCENTAGE
            GUILayout.Label("Percentage");
            popupT = EditorGUILayout.Slider(popupT, 0f, 1f);
            GUILayout.Space(5);

            // ROTATION Z
            GUILayout.Label("Rotation (Z)");
            popupRotationZ = EditorGUILayout.Slider(popupRotationZ, 0f, 360f);

            // SCALE
            GUILayout.Label("Scale");
            popupScale = EditorGUILayout.Slider(popupScale, 0.1f, 0.7f);

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK"))
            {
                ConfirmLevel();
                showPopup = false;
            }

            if (GUILayout.Button("Cancel"))
            {
                DestroyPreviewLevel();
                showPopup = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void ConfirmLevel()
        {
            if (previewLevel == null) return;

            var levels = GetLevels();

            if (selectedLevelIndex >= 0 && selectedLevelIndex < levels.Count)
            {
                var data = levels[selectedLevelIndex];

                data.segmentIndex = popupSegmentIndex;
                data.t = popupT;
                data.rotation = new Vector3(0, 0, popupRotationZ);
                data.scale = Vector3.one * popupScale;

                levels[selectedLevelIndex] = data;
            }
            else
            {
                levels.Add(new MapLevelSpawnData()
                {
                    levelNumber = selectedLevelIndex + 1,
                    segmentIndex = popupSegmentIndex,
                    t = popupT,
                    rotation = new Vector3(0, 0, popupRotationZ),
                    scale = Vector3.one * popupScale,
                    levelVisual = previewLevel
                });
            }

            previewLevel = null;
            selectedLevelIndex = -1;
            EditorUtility.SetDirty(map);
        }
        #endregion

        #region Debug Methods
        void UpdateDebug()
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
                    obj.GetChild(0).localRotation = map.GetTwistRotation(percent);
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
        #endregion
    }
}
#endif
