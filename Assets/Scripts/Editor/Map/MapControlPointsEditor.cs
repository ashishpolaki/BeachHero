using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;

[CustomEditor(typeof(MapEditor))]
public class MapControlPointsEditor : Editor
{
    private bool editMode;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        MapEditor mapTester = (MapEditor)target;

        EditorGUILayout.Space(10);

        // Edit Toggle
        if (GUILayout.Toggle(editMode, "Edit Bezier Points", "Button"))
            editMode = true;
        else
            editMode = false;

        EditorGUILayout.Space();

        // Add Bezier Point
        if (GUILayout.Button("Add Bezier Point"))
        {
            GameObject anchor = new GameObject($"Bezier Anchor {mapTester.bezierPoints.Count}");
            anchor.transform.SetParent(mapTester.bezierPointsParent);
            anchor.transform.position = mapTester.bezierPoints[mapTester.bezierPoints.Count - 1].anchorPoint;

            BezierPoint point = new BezierPoint
            {
                anchorPoint = anchor.transform.position,
                inTangent = Vector3.left,
                outTangent = Vector3.right
            };
            mapTester.bezierPoints.Add(point);

            Undo.RegisterCreatedObjectUndo(anchor, "Add Bezier Point");
            EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
        }

        // Remove Last
        if (GUILayout.Button("Remove Last Bezier Point"))
        {
            if (mapTester.bezierPoints.Count > 0)
            {
                BezierPoint last = mapTester.bezierPoints[mapTester.bezierPoints.Count - 1];
                mapTester.bezierPoints.RemoveAt(mapTester.bezierPoints.Count - 1);
                EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
            }
        }

        // Generate Map
        if (GUILayout.Button("Generate Map"))
        {
            mapTester.GenerateMapPointsInEditor();
            EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
        }

        //Clear Generated Objects
        if (GUILayout.Button("Clear Generated Objects"))
        {
            mapTester.ClearGeneratedObjects();
            EditorSceneManager.MarkSceneDirty(mapTester.gameObject.scene);
        }

        EditorGUILayout.Space(10);
        DrawDefaultInspector();
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
