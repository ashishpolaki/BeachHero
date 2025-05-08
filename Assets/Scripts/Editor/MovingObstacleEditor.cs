#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomEditor(typeof(MovingObstacleEditComponent))]
    public class MovingObstacleEditor : Editor
    {
        #region Variables
        private MovingObstacleEditComponent movingObstacle;
        private bool[] showTangents;
        private bool[] showHandles;
        public static float KeyFramePositionSize = 0.2f;
        public static float KeyFramePositionPickUpSize = 1f;
        public static float keyFrameTangetHandleSize = 0.5f;
        public static float keyFrameTangetCubeSize = 0.1f;
        #endregion

        #region unity methods
        private void OnEnable()
        {
            movingObstacle = (MovingObstacleEditComponent)target;
        }
        #endregion

        #region Inspector Window
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("obstacleType"), new GUIContent("Obstacle Type"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementType"), new GUIContent("Movement Type"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSpeed"), new GUIContent("Movement Speed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("offsetPosition"), new GUIContent("Offset Position"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("offsetRotation"), new GUIContent("Offset Rotation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resolution"), new GUIContent("Resolution"));
            if (movingObstacle.movementType == MovingObstacleMovementType.Circular ||
                          movingObstacle.movementType == MovingObstacleMovementType.FigureEight)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("circleRadius"), new GUIContent("Circle Radius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("circleSegments"), new GUIContent("Circle Segments"));
                if (movingObstacle.movementType == MovingObstacleMovementType.Circular)
                {
                    var keyFrames = BezierCurveUtils.CreateCircleShape(movingObstacle.circleRadius, (int)movingObstacle.circleSegments);
                    movingObstacle.SetKeyFrames(keyFrames);
                }
                else if (movingObstacle.movementType == MovingObstacleMovementType.FigureEight)
                {
                    var keyFrames = BezierCurveUtils.CreateFigureEightShape(movingObstacle.circleRadius, (int)movingObstacle.circleSegments);
                    movingObstacle.SetKeyFrames(keyFrames);
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("loopedMovement"), new GUIContent("Looped Movement"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inverseDirection"), new GUIContent("Inverse Direction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Keyframes"), new GUIContent("Key Frames"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pathPoints"), new GUIContent("Path Positions"),EditorStyles.miniBoldFont);
            movingObstacle.canEditKeyFramesInScene = GUILayout.Toggle(movingObstacle.canEditKeyFramesInScene, "Edit KeyFrames");
            if (GUILayout.Button("Add Keyframe"))
            {
                AddKeyframe();
            }
            if (GUILayout.Button("Remove Last Keyframe"))
            {
                RemoveKeyframe();
            }
            if (GUILayout.Button("Remove All Keyframes"))
            {
                Undo.RecordObject(movingObstacle, "Remove All Keyframes");
                movingObstacle.RemoveAllKeyFrames();
            }
            serializedObject.ApplyModifiedProperties();
        }
        private void AddKeyframe()
        {
            Undo.RecordObject(movingObstacle, "Add Keyframe");

            BezierKeyframe newKeyframe = new BezierKeyframe
            {
                position = Vector3.zero,
                inTangentLocal = Vector3.left,
                outTangentLocal = Vector3.right
            };
            movingObstacle.AddKeyFrame(newKeyframe);
        }
        private void RemoveKeyframe()
        {
            if (movingObstacle.Keyframes == null || movingObstacle.Keyframes.Length == 0)
                return;

            Undo.RecordObject(movingObstacle, "Remove Keyframe");
            movingObstacle.RemoveKeyFrame();
        }
        #endregion

        #region Scene Window
        private void OnSceneGUI()
        {
            if (!movingObstacle.canEditKeyFramesInScene)
                return;

            if (movingObstacle.Keyframes == null || movingObstacle.Keyframes.Length < 2)
                return;

            // Initialize the showTangents array if needed
            if (showTangents == null || showTangents.Length != movingObstacle.Keyframes.Length)
            {
                showTangents = new bool[movingObstacle.Keyframes.Length];
            }
            // Ensure the showHandles array is initialized
            if (showHandles == null || showHandles.Length != movingObstacle.Keyframes.Length)
            {
                showHandles = new bool[movingObstacle.Keyframes.Length];
            }

            for (int i = 0; i < movingObstacle.Keyframes.Length; i++)
            {
                // Draw an interactive Sphere Handle
                Handles.color = Color.green;
                if (Handles.Button(movingObstacle.Keyframes[i].position, Quaternion.identity, KeyFramePositionSize, KeyFramePositionPickUpSize, Handles.SphereHandleCap))
                {
                    // Set all other showHandles to false
                    for (int j = 0; j < showHandles.Length; j++)
                    {
                        showHandles[j] = false;
                    }
                    // Reset all tangent visibility flags
                    for (int j = 0; j < showTangents.Length; j++)
                    {
                        showTangents[j] = false;
                    }

                    // Toggle the visibility of the PositionHandle for this keyframe
                    showHandles[i] = true;
                    // Enable tangent visibility for the current keyframe
                    showTangents[i] = true;
                }

                // Show the PositionHandle only if the button was clicked
                if (showHandles[i])
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPosition = Handles.PositionHandle(movingObstacle.Keyframes[i].position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(movingObstacle, "Move Keyframe");
                        movingObstacle.Keyframes[i].position = newPosition;

                        // Force the Scene view to repaint
                        SceneView.RepaintAll();
                    }
                }

                // Show tangent handlers only if the position handler was interacted with
                if (showTangents[i])
                {
                    // Editable inTangentLocal (Sliders for X, Y, Z axes)
                    Vector3 inTangentWorld = movingObstacle.Keyframes[i].InTangentWorld; // Get the current world position of the inTangent
                    EditorGUI.BeginChangeCheck();
                    Vector3 newInTangentWorld = CustomPositionHandle(inTangentWorld, Quaternion.identity, keyFrameTangetHandleSize, 0.15f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(movingObstacle, "Move In Tangent");

                        // Update the local position of the inTangent
                        movingObstacle.Keyframes[i].inTangentLocal = newInTangentWorld - movingObstacle.Keyframes[i].position;
                    }
                    Handles.color = Color.yellow;
                    // Draw cube for inTangent
                    Handles.CubeHandleCap(0, movingObstacle.Keyframes[i].InTangentWorld, Quaternion.identity, keyFrameTangetCubeSize, EventType.Repaint);


                    // Editable outTangentLocal  (Sliders for X, Y, Z axes)
                    Vector3 outTangentWorld = movingObstacle.Keyframes[i].OutTangentWorld; // Get the current world position of the outTangent
                    EditorGUI.BeginChangeCheck();
                    Vector3 newOutTangentWorld = CustomPositionHandle(outTangentWorld, Quaternion.identity, keyFrameTangetHandleSize, 0.15f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(movingObstacle, "Move Out Tangent");

                        // Update the local position of the outTangent
                        movingObstacle.Keyframes[i].outTangentLocal = newOutTangentWorld - movingObstacle.Keyframes[i].position;
                    }
                    Handles.color = Color.cyan;
                    // Draw cube for inTangent
                    Handles.CubeHandleCap(0, movingObstacle.Keyframes[i].OutTangentWorld, Quaternion.identity, keyFrameTangetCubeSize, EventType.Repaint);

                    // Draw tangent lines
                    Handles.color = Color.white;
                    Handles.DrawLine(movingObstacle.Keyframes[i].position, movingObstacle.Keyframes[i].InTangentWorld);
                    Handles.DrawLine(movingObstacle.Keyframes[i].position, movingObstacle.Keyframes[i].OutTangentWorld);
                }
            }
        }

        private Vector3 CustomPositionHandle(Vector3 position, Quaternion rotation, float handleSize, float rectangleToSliderRatio = 0.15f)
        {
            int controlId1 = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            int controlId2 = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            int controlId3 = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

            float sliderSize = HandleUtility.GetHandleSize(position) * handleSize;
            float rectangleSize = sliderSize * rectangleToSliderRatio;

            Vector3 snap = Vector3.zero;

            Vector3 axis1 = rotation * Vector3.right;
            Vector3 axis2 = rotation * Vector3.up;
            Vector3 axis3 = rotation * Vector3.forward;
            Color axis1Color = new Color(0.9f, 0.3f, 0.1f);
            Color axis2Color = new Color(0.6f, 0.9f, 0.3f);
            Color axis3Color = new Color(0.2f, 0.4f, 0.9f);

            Vector3 updatedPosition = position;

            Handles.color = axis1Color;
            updatedPosition += Handles.Slider(position, axis1, sliderSize, Handles.ArrowHandleCap, snap.x) - position;
            updatedPosition += Handles.Slider2D(controlId1, position, rectangleSize * (axis3 + axis2), Vector3.Cross(axis3, axis2), axis2, axis3, rectangleSize, Handles.RectangleHandleCap, new Vector2(snap.y, snap.z)) - position;

            Handles.color = axis2Color;
            updatedPosition += Handles.Slider(position, axis2, sliderSize, Handles.ArrowHandleCap, snap.y) - position;
            updatedPosition += Handles.Slider2D(controlId2, position, rectangleSize * (axis1 + axis3), Vector3.Cross(axis1, axis3), axis3, axis1, rectangleSize, Handles.RectangleHandleCap, new Vector2(snap.z, snap.x)) - position;

            Handles.color = axis3Color;
            updatedPosition += Handles.Slider(position, axis3, sliderSize, Handles.ArrowHandleCap, snap.z) - position;
            updatedPosition += Handles.Slider2D(controlId3, position, rectangleSize * (axis1 + axis2), Vector3.Cross(axis1, axis2), axis2, axis1, rectangleSize, Handles.RectangleHandleCap, new Vector2(snap.y, snap.x)) - position;

            return updatedPosition;
        }
        #endregion

    }
}
#endif

