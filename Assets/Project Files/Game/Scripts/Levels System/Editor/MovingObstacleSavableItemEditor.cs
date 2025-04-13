using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Watermelon;

namespace Watermelon.BeachRescue
{
    [CustomEditor(typeof(MovingObstacleSavableItem))]
    public class MovingObstacleSavableItemEditor : Editor
    {
        private const string SAVE_PROPERTY_NAME = "save";

        private const string TYPE_PROPERTY_NAME = "type";
        private const string MOVEMENT_SPEED_PROPERTY_NAME = "movementSpeed";
        private const string LOOPED_MOVEMENT_PROPERTY_NAME = "loopedMovement";
        private const string INVERSE_DIRECTION_PROPERTY_NAME = "inverseDirection";

        private const string LINEAR_MOVEMENT_START_POSITION_PROPERTY_NAME = "linearMovementStartPosition";
        private const string LINEAR_MOVEMENT_FINISH_POSITION_PROPERTY_NAME = "linearMovementFinishPosition";

        private const string CIRCLE_CENTER_PROPERTY_NAME = "circlarMovementCenter";
        private const string CIRCLE_RADIUS_PROPERTY_NAME = "circlarMovementRadius";

        private SerializedProperty saveProp;

        private SerializedProperty typeProp;
        private SerializedProperty movementSpeedProp;
        private SerializedProperty loopedMovementProp;
        private SerializedProperty inverseDirectionProp;

        private SerializedProperty linearMovementStartPosProp;
        private SerializedProperty linearMovementFinishPosProp;

        private SerializedProperty circleCenterProp;
        private SerializedProperty circleRadiusProp;

        bool isHandlesDisabled;

        protected void OnEnable()
        {
            saveProp = serializedObject.FindProperty(SAVE_PROPERTY_NAME);
            typeProp = saveProp.FindPropertyRelative(TYPE_PROPERTY_NAME);
            movementSpeedProp = saveProp.FindPropertyRelative(MOVEMENT_SPEED_PROPERTY_NAME);
            loopedMovementProp = saveProp.FindPropertyRelative(LOOPED_MOVEMENT_PROPERTY_NAME);
            inverseDirectionProp = saveProp.FindPropertyRelative(INVERSE_DIRECTION_PROPERTY_NAME);

            linearMovementStartPosProp = saveProp.FindPropertyRelative(LINEAR_MOVEMENT_START_POSITION_PROPERTY_NAME);
            linearMovementFinishPosProp = saveProp.FindPropertyRelative(LINEAR_MOVEMENT_FINISH_POSITION_PROPERTY_NAME);

            circleCenterProp = saveProp.FindPropertyRelative(CIRCLE_CENTER_PROPERTY_NAME);
            circleRadiusProp = saveProp.FindPropertyRelative(CIRCLE_RADIUS_PROPERTY_NAME);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(typeProp);
            EditorGUILayout.PropertyField(movementSpeedProp);
            EditorGUILayout.PropertyField(loopedMovementProp);
            EditorGUILayout.PropertyField(inverseDirectionProp);

            if (typeProp.intValue == (int)MovingObstacleType.Linear)
            {
                EditorGUILayout.PropertyField(linearMovementStartPosProp);
                EditorGUILayout.PropertyField(linearMovementFinishPosProp);
            }
            else if (typeProp.intValue == (int)MovingObstacleType.Circle)
            {
                EditorGUILayout.PropertyField(circleCenterProp);
                EditorGUILayout.PropertyField(circleRadiusProp);
            }



            serializedObject.ApplyModifiedProperties();
            isHandlesDisabled = GUILayout.Toggle(isHandlesDisabled, "isHandlesDisabled");
        }

        public void OnSceneGUI()
        {

            if (isHandlesDisabled || serializedObject == null)
            {
                return;
            }

            if (typeProp.intValue == (int)MovingObstacleType.Linear)
            {

                linearMovementStartPosProp.vector3Value = Handles.PositionHandle(linearMovementStartPosProp.vector3Value, Quaternion.identity);

                Handles.SphereHandleCap(0, linearMovementFinishPosProp.vector3Value, Quaternion.identity, 0.7f, EventType.Repaint);
                linearMovementFinishPosProp.vector3Value = Handles.PositionHandle(linearMovementFinishPosProp.vector3Value, Quaternion.identity);


            }
            else if (typeProp.intValue == (int)MovingObstacleType.Circle)
            {
                Handles.SphereHandleCap(0, circleCenterProp.vector3Value, Quaternion.identity, 0.7f, EventType.Repaint);
                circleCenterProp.vector3Value = Handles.PositionHandle(circleCenterProp.vector3Value, Quaternion.identity);

                circleRadiusProp.floatValue = Handles.RadiusHandle(Quaternion.identity, circleCenterProp.vector3Value, circleRadiusProp.floatValue);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}