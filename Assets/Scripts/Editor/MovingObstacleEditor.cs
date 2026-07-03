#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomEditor(typeof(MovingObstacleEditTool))]
    public class MovingObstacleEditor : Editor
    {
        #region Variables
        private MovingObstacleEditTool movingObstacle;
        private MovingObstacleShape movementShape;
        private int addKeyframeIndex = 0;
        private int removeKeyframeIndex = 0;
        private int segments;
        private float radius;
        private bool showGenerateShapeSettings = false;
        private bool showOffsetSettings = false;
        public static float KeyFramePositionSize = 0.2f;
        public static float KeyFramePositionPickUpSize = 1f;
        public static float keyFrameTangetHandleSize = 0.5f;
        public static float keyFrameTangetCubeSize = 0.1f;
        #endregion

        #region unity methods
        private void OnEnable()
        {
            movingObstacle = (MovingObstacleEditTool)target;
        }
        #endregion

        #region Inspector Window
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("obstacleType"), new GUIContent("Obstacle Type"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSpeed"), new GUIContent("Movement Speed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeedMultiplier"), new GUIContent("Rotation Speed Multiplier"));
         //   EditorGUILayout.PropertyField(serializedObject.FindProperty("offsetPosition"), new GUIContent("Offset Position"));
         //   EditorGUILayout.PropertyField(serializedObject.FindProperty("offsetRotation"), new GUIContent("Offset Rotation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resolution"), new GUIContent("Resolution"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("loopedMovement"), new GUIContent("Looped Movement"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inverseDirection"), new GUIContent("Inverse Direction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canEditKeyFramesInScene"), new GUIContent("Edit KeyFrames"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Keyframes"), new GUIContent("Key Frames"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pathPoints"), new GUIContent("Path Positions"), EditorStyles.miniBoldFont);
            if (movingObstacle.canEditKeyFramesInScene)
            {
                AddKeyframe();
                RemoveKeyframe();
                AddKeyframeAtIndex();
                RemoveKeyframeAtIndex();
                RemoveAllKeyframes();
                DrawShapeGenerator();
                DrawOffsetToggle();
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(movingObstacle);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
        #region Keyframe Management
        private void DrawOffsetToggle()
        {
            // EditorGUILayout.Space(1);
            // Toggle with Button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(showOffsetSettings ? "Hide Offset" : "Enable Offset", GUILayout.Height(22), GUILayout.Width(220)))
            {
                showOffsetSettings = !showOffsetSettings;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void HandleKeyframeTransform()
        {
            if (showOffsetSettings)
            {
                Vector3 center = Vector3.zero;
                for (int i = 0; i < movingObstacle.Keyframes.Length; i++)
                    center += movingObstacle.Keyframes[i].position;
                center /= movingObstacle.Keyframes.Length;

                // 2. Draw offset handle at the center
                Handles.color = Color.blue;
                EditorGUI.BeginChangeCheck();
                Vector3 newCenter = Handles.FreeMoveHandle(center, 1f, Vector3.zero, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(movingObstacle, "Move All Keyframes Offset");
                    Vector3 delta = newCenter - center;
                    // Apply movement to all keyframes
                    for (int i = 0; i < movingObstacle.Keyframes.Length; i++)
                        movingObstacle.Keyframes[i].position += delta;
                }

                // 3. Rotation handle at the same center
                EditorGUI.BeginChangeCheck();
                Quaternion newRot = Handles.RotationHandle(Quaternion.identity, center);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(movingObstacle, "Rotate All Keyframes");
                    for (int i = 0; i < movingObstacle.Keyframes.Length; i++)
                    {
                        Vector3 dir = movingObstacle.Keyframes[i].position - center;
                        dir = newRot * dir; // rotate around center
                        movingObstacle.Keyframes[i].position = center + dir;
                        movingObstacle.Keyframes[i].inTangentLocal = newRot * movingObstacle.Keyframes[i].inTangentLocal;
                        movingObstacle.Keyframes[i].outTangentLocal = newRot * movingObstacle.Keyframes[i].outTangentLocal;
                    }
                }
            }
        }

        private void HandleShapeGeneration()
        {
            if (movementShape == MovingObstacleShape.Circular)
            {
                var keyFrames = BezierCurveUtils.CreateCircleShape(radius, segments);
                movingObstacle.SetKeyFrames(keyFrames);
            }
            else if (movementShape == MovingObstacleShape.FigureEight)
            {
                var keyFrames = BezierCurveUtils.CreateFigureEightShape(radius, segments);
                movingObstacle.SetKeyFrames(keyFrames);
            }
            EditorUtility.SetDirty(movingObstacle);
        }

        private void DrawShapeGenerator()
        {
            EditorGUILayout.Space(5);

            // Toggle with Button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(showGenerateShapeSettings ? "Hide Generate Shape Settings" : "Show Generate Shape Settings", GUILayout.Height(22), GUILayout.Width(220)))
            {
                showGenerateShapeSettings = !showGenerateShapeSettings;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (showGenerateShapeSettings)
            {
                Rect rect = EditorGUILayout.BeginVertical();

                // Header Text (centered)
                GUIStyle centeredBoldLabel = new GUIStyle(EditorStyles.boldLabel);
                centeredBoldLabel.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("Generate Shape Settings", centeredBoldLabel);

                EditorGUILayout.Space(3);
                movementShape = (MovingObstacleShape)EditorGUILayout.EnumPopup("Shape Type", movementShape);
                radius = EditorGUILayout.FloatField("Radius", radius);
                segments = EditorGUILayout.IntField("Segments", segments);
                movingObstacle.resolution = EditorGUILayout.FloatField("Resolution", movingObstacle.resolution);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Generate", GUILayout.Height(20), GUILayout.Width(120)))
                {
                    Undo.RecordObject(movingObstacle, "Generate Shape");
                    HandleShapeGeneration();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                EditorGUILayout.Space(3);
                EditorGUILayout.EndVertical();

                // Draw outline after layout
                Handles.BeginGUI();
                Handles.color = Color.yellow;
                Handles.DrawAAPolyLine(2,
                    new Vector3(rect.xMin, rect.yMin),
                    new Vector3(rect.xMax, rect.yMin),
                    new Vector3(rect.xMax, rect.yMax),
                    new Vector3(rect.xMin, rect.yMax),
                    new Vector3(rect.xMin, rect.yMin));
                Handles.EndGUI();
            }
        }

        private void AddKeyframe()
        {
            if (GUILayout.Button("Add Keyframe"))
            {
                Undo.RecordObject(movingObstacle, "Add Keyframe");

                BezierKeyframe newKeyframe;

                int count = movingObstacle.Keyframes.Length;

                // No points
                if (count == 0)
                {
                    newKeyframe = new BezierKeyframe
                    {
                        position = Vector3.zero,
                        inTangentLocal = Vector3.left * 0.5f,
                        outTangentLocal = Vector3.right * 0.5f
                    };
                }
                // Only one point
                else if (count == 1)
                {
                    Vector3 p = movingObstacle.Keyframes[0].position;

                    newKeyframe = new BezierKeyframe
                    {
                        position = p + Vector3.right,
                        inTangentLocal = Vector3.left * 0.5f,
                        outTangentLocal = Vector3.right * 0.5f
                    };
                }
                // Two or more points
                else
                {
                    var prev = movingObstacle.Keyframes[count - 2];
                    var last = movingObstacle.Keyframes[count - 1];

                    Vector3 dir = (last.position - prev.position).normalized;

                    if (dir == Vector3.zero)
                        dir = Vector3.right;

                    newKeyframe = new BezierKeyframe
                    {
                        position = last.position + dir,
                        inTangentLocal = -dir * 0.5f,
                        outTangentLocal = dir * 0.5f
                    };
                }
                movingObstacle.AddKeyFrame(newKeyframe);
            }
        }

        private void RemoveKeyframe()
        {
            // Remove Last Keyframe
            if (GUILayout.Button("Remove Last Keyframe"))
            {
                Undo.RecordObject(movingObstacle, "Remove Last Keyframe");
                if (movingObstacle.Keyframes == null || movingObstacle.Keyframes.Length == 0)
                    return;

                Undo.RecordObject(movingObstacle, "Remove Keyframe");
                movingObstacle.RemoveKeyFrame();
            }
        }

        private void AddKeyframeAtIndex()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Keyframe At Index"))
            {
                Undo.RecordObject(movingObstacle, "Add Keyframe At Index");

                if (movingObstacle.Keyframes == null || addKeyframeIndex < 0 || addKeyframeIndex > movingObstacle.Keyframes.Length)
                    return;

                Undo.RecordObject(movingObstacle, "Add Keyframe At Index");

                BezierKeyframe newKeyframe;

                int count = movingObstacle.Keyframes.Length;

                // Insert between two existing points
                if (addKeyframeIndex < count - 1)
                {
                    var a = movingObstacle.Keyframes[addKeyframeIndex];
                    var b = movingObstacle.Keyframes[addKeyframeIndex + 1];

                    Vector3 dir = (b.position - a.position).normalized;

                    if (dir == Vector3.zero)
                        dir = Vector3.right;

                    newKeyframe = new BezierKeyframe
                    {
                        position = (a.position + b.position) * 0.5f,
                        inTangentLocal = -dir * 0.5f,
                        outTangentLocal = dir * 0.5f
                    };
                }
                // Insert after the last point
                else
                {
                    if (count == 1)
                    {
                        Vector3 p = movingObstacle.Keyframes[0].position;

                        newKeyframe = new BezierKeyframe
                        {
                            position = p + Vector3.right,
                            inTangentLocal = Vector3.left * 0.5f,
                            outTangentLocal = Vector3.right * 0.5f
                        };
                    }
                    else
                    {
                        var prev = movingObstacle.Keyframes[count - 2];
                        var last = movingObstacle.Keyframes[count - 1];

                        Vector3 dir = (last.position - prev.position).normalized;

                        if (dir == Vector3.zero)
                            dir = Vector3.right;

                        newKeyframe = new BezierKeyframe
                        {
                            position = last.position + dir,
                            inTangentLocal = -dir * 0.5f,
                            outTangentLocal = dir * 0.5f
                        };
                    }
                }
                movingObstacle.AddKeyframeAtIndex(addKeyframeIndex, newKeyframe);
            }
            GUILayout.Label("Index:", GUILayout.Width(50));
            addKeyframeIndex = EditorGUILayout.IntField(addKeyframeIndex, GUILayout.Width(50));
            GUILayout.EndHorizontal();
        }

        private void RemoveKeyframeAtIndex()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove Keyframe At Index"))
            {
                Undo.RecordObject(movingObstacle, "Remove Keyframe At Index");
                if (movingObstacle.Keyframes == null || movingObstacle.Keyframes.Length == 0 || removeKeyframeIndex < 0 || removeKeyframeIndex >= movingObstacle.Keyframes.Length)
                    return;

                Undo.RecordObject(movingObstacle, "Remove Keyframe At Index");
                movingObstacle.RemoveKeyframeAtIndex(removeKeyframeIndex);
            }
            GUILayout.Label("Index:", GUILayout.Width(50));
            removeKeyframeIndex = EditorGUILayout.IntField(removeKeyframeIndex, GUILayout.Width(50));
            GUILayout.EndHorizontal();
        }

        private void RemoveAllKeyframes()
        {
            if (GUILayout.Button("Remove All Keyframes"))
            {
                Undo.RecordObject(movingObstacle, "Remove All Keyframes");
                movingObstacle.RemoveAllKeyFrames();
            }
        }

        #endregion

        #endregion

        #region Scene Window
        private void OnSceneGUI()
        {
            if (!movingObstacle.canEditKeyFramesInScene)
                return;

            if (movingObstacle.Keyframes.Length == 1)
            {
                Handles.Label(movingObstacle.Keyframes[0].position + Vector3.up * 0.5f, $"Point {0}",
                   new GUIStyle
                   {
                       fontSize = 15,
                       normal = new GUIStyleState { textColor = Color.white }
                   });

                // Draw an interactive Sphere Handle
                Handles.color = Color.white;
                EditorGUI.BeginChangeCheck();
                Vector3 newKeyFramePos = Handles.FreeMoveHandle(movingObstacle.Keyframes[0].position, 0.15f, Vector3.zero, Handles.SphereHandleCap);
                bool keyFrameMoved = EditorGUI.EndChangeCheck();
                if (keyFrameMoved)
                {
                    Undo.RecordObject(movingObstacle, "Move Keyframe");
                    movingObstacle.Keyframes[0].position = newKeyFramePos;

                    // Force the Scene view to repaint
                    SceneView.RepaintAll();
                }
            }

            if (movingObstacle.Keyframes == null || movingObstacle.Keyframes.Length < 2)
                return;

            for (int i = 0; i < movingObstacle.Keyframes.Length; i++)
            {
                // Display the index of the keyframe as a label in the scene
                Handles.Label(movingObstacle.Keyframes[i].position + Vector3.up * 0.5f, $"Point {i}",
                    new GUIStyle
                    {
                        fontSize = 15,
                        normal = new GUIStyleState { textColor = Color.white }
                    });

                // Draw an interactive Sphere Handle
                Handles.color = Color.white;
                EditorGUI.BeginChangeCheck();
                Vector3 newKeyFramePos = Handles.FreeMoveHandle(movingObstacle.Keyframes[i].position, 0.15f, Vector3.zero, Handles.SphereHandleCap);
                bool keyFrameMoved = EditorGUI.EndChangeCheck();
                if (keyFrameMoved)
                {
                    Undo.RecordObject(movingObstacle, "Move Keyframe");
                    movingObstacle.Keyframes[i].position = newKeyFramePos;

                    // Force the Scene view to repaint
                    SceneView.RepaintAll();
                }

                // In-Tangent
                Handles.color = Color.green;
                Vector3 anchorPos = movingObstacle.Keyframes[i].position;
                Vector3 inWorld = movingObstacle.Keyframes[i].InTangentWorld;
                EditorGUI.BeginChangeCheck();
                Vector3 newInWorld = Handles.FreeMoveHandle(inWorld, 0.1f, Vector3.zero, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(movingObstacle, "Move In-Tangent");
                    movingObstacle.Keyframes[i].inTangentLocal = newInWorld - movingObstacle.Keyframes[i].position;
                }
                Handles.DrawLine(anchorPos, inWorld);

                //Out-Tangent
                Handles.color = Color.red;
                Vector3 outWorld = movingObstacle.Keyframes[i].OutTangentWorld;
                EditorGUI.BeginChangeCheck();
                Vector3 newOutWorld = Handles.FreeMoveHandle(outWorld, 0.1f, Vector3.zero, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(movingObstacle, "Move Out-Tangent");
                    movingObstacle.Keyframes[i].outTangentLocal = newOutWorld - movingObstacle.Keyframes[i].position;
                }
                Handles.DrawLine(anchorPos, outWorld);
            }

            //Offset
            HandleKeyframeTransform();
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

