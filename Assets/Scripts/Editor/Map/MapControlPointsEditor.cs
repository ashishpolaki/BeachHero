#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BeachHero
{
    [CustomEditor(typeof(MapEditor))]
    public class MapControlPointsEditor : Editor
    {
        private bool editMode;
        private SerializedProperty bezierPointsProperty;
        private int addPointIndex = 0;
        private int removePointIndex = 0;
        private int resizePointsCount = 1;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MapEditor mapTester = (MapEditor)target;

            // Edit Toggle
            EditorGUILayout.Space(10);
            if (GUILayout.Toggle(editMode, "Edit Bezier Points", "Button"))
                editMode = true;
            else
                editMode = false;
            EditorGUILayout.Space();

            if (editMode)
            {
                // Add Bezier Point at End
                if (GUILayout.Button("Add Point at End"))
                {
                    var anchorPoint = mapTester.bezierPoints.Count > 0 ? mapTester.bezierPoints[mapTester.bezierPoints.Count - 1].anchorPoint : Vector3.zero;
                    var inTangent = mapTester.bezierPoints.Count > 0 ? mapTester.bezierPoints[mapTester.bezierPoints.Count - 1].inTangent : Vector3.zero;
                    var outTangent = mapTester.bezierPoints.Count > 0 ? mapTester.bezierPoints[mapTester.bezierPoints.Count - 1].outTangent : Vector3.zero;

                    BezierPoint point = new BezierPoint
                    {
                        anchorPoint = anchorPoint,
                        inTangent = inTangent,
                        outTangent = outTangent
                    };
                    mapTester.bezierPoints.Add(point);

                    Undo.RegisterCreatedObjectUndo(mapTester, "Add Bezier Point");
                    EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
                }

                // Remove Point at End
                if (GUILayout.Button("Remove Point At End"))
                {
                    if (mapTester.bezierPoints.Count > 0)
                    {
                        mapTester.bezierPoints.RemoveAt(mapTester.bezierPoints.Count - 1);
                        EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
                    }
                }

                //Add Bezier Point at Given Index
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Point at Index"))
                {
                    //  int addPointIndex = EditorGUILayout.IntField("Index", mapTester.bezierPoints.Count);
                    if (addPointIndex < 0 || addPointIndex > mapTester.bezierPoints.Count)
                    {
                        DebugUtils.LogError("Index out of range. Please enter a valid index.");
                    }
                    else
                    {
                        var anchorPoint = mapTester.bezierPoints.Count > 0 ? mapTester.bezierPoints[addPointIndex].anchorPoint : Vector3.zero;
                        var inTangent = mapTester.bezierPoints.Count > 0 ? mapTester.bezierPoints[addPointIndex].inTangent : Vector3.zero;
                        var outTangent = mapTester.bezierPoints.Count > 0 ? mapTester.bezierPoints[addPointIndex].outTangent : Vector3.zero;
                        BezierPoint point = new BezierPoint
                        {
                            anchorPoint = anchorPoint,
                            inTangent = inTangent,
                            outTangent = outTangent
                        };
                        mapTester.bezierPoints.Insert(addPointIndex, point);
                        Undo.RegisterCreatedObjectUndo(mapTester, "Add Bezier Point at Index");
                        EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
                    }
                }
                GUILayout.Label("Index", GUILayout.Width(60));
                addPointIndex = EditorGUILayout.IntField(addPointIndex, GUILayout.Width(60));
                GUILayout.EndHorizontal();

                //Remove Bezier Point at Given Index
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove Point at Index"))
                {
                    if (removePointIndex < 0 || removePointIndex >= mapTester.bezierPoints.Count)
                    {
                        DebugUtils.LogError("Index out of range. Please enter a valid index.");
                    }
                    else
                    {
                        mapTester.bezierPoints.RemoveAt(removePointIndex);
                        EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
                    }
                }
                GUILayout.Label("Index", GUILayout.Width(60));
                removePointIndex = EditorGUILayout.IntField(removePointIndex, GUILayout.Width(60));
                GUILayout.EndHorizontal();

                //Resize Bezier Points
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Resize Bezier Points"))
                {
                    mapTester.resizeLevels = resizePointsCount;
                    mapTester.GenerateMapPointsInEditor(true);
                    EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
                }
                GUILayout.Label("Count", GUILayout.Width(60));
                resizePointsCount = EditorGUILayout.IntField(resizePointsCount, GUILayout.Width(60));
                GUILayout.EndHorizontal();

                //Clear Generated Objects
                if (GUILayout.Button("Clear"))
                {
                    mapTester.ClearGeneratedObjects();
                    EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
                }

                // Generate Map
                if (GUILayout.Button("Generate Map Path"))
                {
                    mapTester.GenerateMapPointsInEditor(false);
                    EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
                }

                //Generate Level Visuals 
                if (GUILayout.Button("Generate Level Visuals"))
                {
                    mapTester.GenerateLevelVisuals();
                    EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
                }

                //Show mapTester bezier Points like a serialized Property
                // --- Draw the List in Inspector ---
                bezierPointsProperty = serializedObject.FindProperty("bezierPoints");
                EditorGUILayout.PropertyField(bezierPointsProperty, new GUIContent("BezierPoints"), true);
            }
            else
            {
                DrawDefaultInspector();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            MapEditor mapTester = (MapEditor)target;
            if (!editMode || mapTester.bezierPoints == null)
                return;

            for (int i = 0; i < mapTester.bezierPoints.Count; i++)
            {
                var point = mapTester.bezierPoints[i];
                if (point.anchorPoint == null) continue;

                Vector3 oldAnchorPos = point.anchorPoint;
                // === 1. MOVE ANCHOR FIRST ===
                Handles.color = Color.white;
                EditorGUI.BeginChangeCheck();
                Vector3 newAnchorPos = Handles.FreeMoveHandle(
                    oldAnchorPos,
                    0.15f,
                    Vector3.zero,
                    Handles.SphereHandleCap
                );
                bool anchorMoved = EditorGUI.EndChangeCheck();

                if (anchorMoved)
                {
                    Undo.RecordObject(mapTester, "Move Anchor");
                    Vector3 delta = newAnchorPos - oldAnchorPos;
                    point.anchorPoint = newAnchorPos;
                }
                GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    normal = { textColor = Color.red }
                };
                Handles.Label(oldAnchorPos + Vector3.up * 0.2f, $"#{i}", labelStyle); // or $"{i}" for just index

                if (!anchorMoved)
                {
                    // === 2. UPDATE IN/OUT TANGENTS ===
                    Vector3 anchorPos = point.anchorPoint;

                    // In-Tangent
                    Handles.color = Color.green;
                    Vector3 inWorld = anchorPos + point.inTangent;
                    EditorGUI.BeginChangeCheck();
                    Vector3 newInWorld = Handles.FreeMoveHandle(
                        inWorld,
                        0.1f,
                        Vector3.zero,
                        Handles.SphereHandleCap
                    );
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(mapTester, "Move In-Tangent");
                        point.inTangent = newInWorld - anchorPos;
                    }
                    Handles.DrawLine(anchorPos, inWorld);

                    // Out-Tangent
                    Handles.color = Color.red;
                    Vector3 outWorld = anchorPos + point.outTangent;
                    EditorGUI.BeginChangeCheck();
                    Vector3 newOutWorld = Handles.FreeMoveHandle(
                        outWorld,
                        0.1f,
                        Vector3.zero,
                        Handles.SphereHandleCap
                    );
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(mapTester, "Move Out-Tangent");
                        point.outTangent = newOutWorld - anchorPos;
                    }
                    Handles.DrawLine(anchorPos, outWorld);
                }

                // Mark scene dirty
                if (GUI.changed)
                {
                    EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
                }
            }
        }
    }
}
#endif
