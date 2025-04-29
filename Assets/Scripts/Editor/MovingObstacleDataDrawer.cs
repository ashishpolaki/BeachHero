#if  UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(MovingObstacleData))]
    public class MovingObstacleDataDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Calculate the height dynamically
            int lines = 6; // Always visible: movement type, linear positions, looped/inverse bools
            int circularLines = 2; // Circular center and radius
            int linearLines = 2; // Linear start and finish positions

            SerializedProperty movementTypeProp = property.FindPropertyRelative("movementType");
            if ((MovingObstacleMovementType)movementTypeProp.enumValueIndex == MovingObstacleMovementType.Linear)
            {
                lines += circularLines; // Add circular fields if NOT linear
            }
            if ((MovingObstacleMovementType)movementTypeProp.enumValueIndex == MovingObstacleMovementType.Circular)
            {
                lines += linearLines; // Add circular fields if NOT linear
            }

            return EditorGUIUtility.singleLineHeight * lines + 10f; // some spacing
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
            float bigSpacing = 8f;

            Rect rect = new Rect(position.x, position.y, position.width, lineHeight);

            // Fields
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            SerializedProperty movementTypeProp = property.FindPropertyRelative("movementType");
            SerializedProperty linearStartProp = property.FindPropertyRelative("linearMovementStartPosition");
            SerializedProperty linearFinishProp = property.FindPropertyRelative("linearMovementFinishPosition");
            SerializedProperty circularCenterProp = property.FindPropertyRelative("circlarMovementCenter");
            SerializedProperty circularRadiusProp = property.FindPropertyRelative("circlarMovementRadius");
            SerializedProperty loopedProp = property.FindPropertyRelative("loopedMovement");
            SerializedProperty inverseProp = property.FindPropertyRelative("inverseDirection");

            EditorGUI.PropertyField(rect, typeProp);
            rect.y += lineHeight + verticalSpacing;
            EditorGUI.PropertyField(rect, movementTypeProp);
            rect.y += lineHeight + bigSpacing;

            if ((MovingObstacleMovementType)movementTypeProp.enumValueIndex == MovingObstacleMovementType.Linear)
            {
                EditorGUI.PropertyField(rect, linearStartProp);
                rect.y += lineHeight + bigSpacing + verticalSpacing;
                EditorGUI.PropertyField(rect, linearFinishProp);
                rect.y += lineHeight + bigSpacing + verticalSpacing;
            }

            if ((MovingObstacleMovementType)movementTypeProp.enumValueIndex == MovingObstacleMovementType.Circular)
            {
                EditorGUI.PropertyField(rect, circularCenterProp);
                rect.y += lineHeight + bigSpacing;

                EditorGUI.PropertyField(rect, circularRadiusProp);
                rect.y += lineHeight + verticalSpacing;
            }

            EditorGUI.PropertyField(rect, loopedProp);
            rect.y += lineHeight + bigSpacing;
            EditorGUI.PropertyField(rect, inverseProp);
            rect.y += lineHeight + bigSpacing;
            EditorGUI.EndProperty();
        }
    }
}
#endif
