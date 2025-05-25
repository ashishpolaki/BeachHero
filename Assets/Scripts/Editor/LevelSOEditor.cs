#if  UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomEditor(typeof(LevelSO))]
    public class LevelSOEditor : Editor
    {
        private LevelSO levelSO;
        private Vector2 movingObstaclesScrollPosition;

        private void OnEnable()
        {
            levelSO = (LevelSO)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("levelTime"), new GUIContent("Level Time"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startPoint"), new GUIContent("Start Point"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("drownCharacters"), new GUIContent("Drown Characters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("collectables"), new GUIContent("Collectables"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("obstacles").FindPropertyRelative("staticObstacles"), new GUIContent("Static Obstacles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("obstacles").FindPropertyRelative("waterHoleObstacles"), new GUIContent("Cyclone Obstacles"));
            EditMovingObstacleProperty(serializedObject.FindProperty("obstacles").FindPropertyRelative("movingObstacles"));
            serializedObject.ApplyModifiedProperties();
        }
        private void EditMovingObstacleProperty(SerializedProperty movingObstaclesProperty)
        {
            //  EditorGUILayout.LabelField("Moving Obstacles", EditorStyles.boldLabel);
            movingObstaclesProperty.isExpanded = EditorGUILayout.Foldout(movingObstaclesProperty.isExpanded, "Moving Obstacles", true, EditorStyles.foldout);

            if (movingObstaclesProperty.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Display the size of the array
                EditorGUILayout.PropertyField(movingObstaclesProperty.FindPropertyRelative("Array.size"), new GUIContent("Size"));

                // Calculate the total height dynamically based on the number of elements and their expanded state
                int totalElements = movingObstaclesProperty.arraySize;
                float totalHeight = 0;

                for (int i = 0; i < totalElements; i++)
                {
                    SerializedProperty element = movingObstaclesProperty.GetArrayElementAtIndex(i);
                    if (element.isExpanded)
                    {
                        totalHeight += 200; // Approximate height for an expanded element
                    }
                    else
                    {
                        totalHeight += 18; // Approximate height for a folded element
                    }
                }

                // Clamp the height to avoid excessive growth
                float scrollViewHeight = Mathf.Clamp(totalHeight, 0, 500); // Minimum 100, maximum 500

                // Begin the scroll view
                movingObstaclesScrollPosition = EditorGUILayout.BeginScrollView(movingObstaclesScrollPosition, GUILayout.Height(scrollViewHeight));

                // Iterate through the array and display each element
                for (int i = 0; i < movingObstaclesProperty.arraySize; i++)
                {
                    MovingObstacleData movingObstacleData = levelSO.Obstacle.MovingObstacles[i];
                    SerializedProperty element = movingObstaclesProperty.GetArrayElementAtIndex(i);

                    // Add a foldout for each element
                    element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, $"Element {i}", true);

                    if (element.isExpanded)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        // Display each field of MovingObstacleData
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("type"), new GUIContent("Obstacle Type"));
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("movementType"), new GUIContent("Movement Type"));
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("loopedMovement"), new GUIContent("Looped Movement"));
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("inverseDirection"), new GUIContent("Inverse Direction"));
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("movementSpeed"), new GUIContent("Movement Speed"));
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("resolution"), new GUIContent("Resolution"));

                        EditorGUILayout.PropertyField(element.FindPropertyRelative("bezierKeyframes"), new GUIContent("Bezier Keyframes"));

                        // Add a "Remove" button
                        if (GUILayout.Button("Remove Element", GUILayout.Width(120)))
                        {
                            movingObstaclesProperty.DeleteArrayElementAtIndex(i);
                            break; // Exit the loop to avoid issues with array size changes
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }
                }

                EditorGUILayout.EndScrollView();
                if (GUILayout.Button("Add Element", GUILayout.Width(120)))
                {
                    movingObstaclesProperty.InsertArrayElementAtIndex(movingObstaclesProperty.arraySize);
                }
                EditorGUI.indentLevel--;
            }
        }
    }
    // [CustomPropertyDrawer(typeof(MovingObstacleData))]
    // public class MovingObstacleDataDrawer : PropertyDrawer
    // {
    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //{
    //    // Calculate the height dynamically
    //    int lines = 8; // Always visible
    //    int circularLines = 2; // Circular center and radius

    //    SerializedProperty movementTypeProp = property.FindPropertyRelative("movementType");
    //    if ((MovingObstacleMovementType)movementTypeProp.enumValueIndex == MovingObstacleMovementType.Circular || (MovingObstacleMovementType)movementTypeProp.enumValueIndex == MovingObstacleMovementType.FigureEight)
    //    {
    //        lines += circularLines; // Add circular fields if NOT linear
    //    }
    //    else if ((MovingObstacleMovementType)movementTypeProp.enumValueIndex == MovingObstacleMovementType.Custom)
    //    {
    //        lines += 2; // Add bezier Keyframes and resolution
    //    }

    //    return EditorGUIUtility.singleLineHeight * lines + 10f; // some spacing
    //}

    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //{
    //    EditorGUI.BeginProperty(position, label, property);

    //    float lineHeight = EditorGUIUtility.singleLineHeight;
    //    float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
    //    float bigSpacing = 8f;

    //    Rect rect = new Rect(position.x, position.y, position.width, lineHeight);

    //    // Fields
    //    SerializedProperty typeProp = property.FindPropertyRelative("type");
    //    SerializedProperty movementTypeProp = property.FindPropertyRelative("movementType");
    //    SerializedProperty circleRadiusProp = property.FindPropertyRelative("circleRadius");
    //    SerializedProperty segmentsProp = property.FindPropertyRelative("circleSegments");
    //    SerializedProperty loopedProp = property.FindPropertyRelative("loopedMovement");
    //    SerializedProperty inverseProp = property.FindPropertyRelative("inverseDirection");
    //    SerializedProperty bezierKeyframesProp = property.FindPropertyRelative("bezierKeyframes");
    //    SerializedProperty resolutionProp = property.FindPropertyRelative("resolution");
    //    SerializedProperty offsetPositionProp = property.FindPropertyRelative("offsetPosition");
    //    SerializedProperty offsetRotationProp = property.FindPropertyRelative("offsetRotation");

    //    //Position and Rotation offset
    //    EditorGUI.PropertyField(rect, offsetPositionProp);
    //    rect.y += lineHeight + verticalSpacing;
    //    EditorGUI.PropertyField(rect, offsetRotationProp);
    //    rect.y += lineHeight + verticalSpacing;

    //    EditorGUI.PropertyField(rect, typeProp);
    //    rect.y += lineHeight + verticalSpacing;
    //    EditorGUI.PropertyField(rect, movementTypeProp);
    //    rect.y += lineHeight + bigSpacing;

    //    if ((MovingObstacleMovementType)movementTypeProp.enumValueIndex == MovingObstacleMovementType.Circular)
    //    {
    //        EditorGUI.PropertyField(rect, circleRadiusProp);
    //        rect.y += lineHeight + verticalSpacing;

    //        EditorGUI.PropertyField(rect, segmentsProp);
    //        rect.y += lineHeight + verticalSpacing;
    //    }

    //    if ((MovingObstacleMovementType)movementTypeProp.enumValueIndex == MovingObstacleMovementType.Custom)
    //    {
    //        EditorGUI.PropertyField(rect, circleRadiusProp);
    //        rect.y += lineHeight + verticalSpacing;

    //        EditorGUI.PropertyField(rect, segmentsProp);
    //        rect.y += lineHeight + verticalSpacing;
    //    }

    //    if ((MovingObstacleMovementType)movementTypeProp.enumValueIndex == MovingObstacleMovementType.FigureEight)
    //    {
    //        EditorGUI.PropertyField(rect, circleRadiusProp);
    //        rect.y += lineHeight + verticalSpacing;

    //        EditorGUI.PropertyField(rect, segmentsProp);
    //        rect.y += lineHeight + verticalSpacing;
    //    }

    //    EditorGUI.PropertyField(rect, loopedProp);
    //    rect.y += lineHeight + verticalSpacing;
    //    EditorGUI.PropertyField(rect, inverseProp);
    //    rect.y += lineHeight + verticalSpacing;
    //    EditorGUI.EndProperty();
    //}
    // }
}
#endif
