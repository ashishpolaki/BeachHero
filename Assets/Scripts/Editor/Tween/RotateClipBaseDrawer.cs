#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(RotateClipBase), true)]
    public class RotateClipBaseDrawer : TweenClipBaseDrawer
    {
        static Dictionary<string, Object> targetCache = new Dictionary<string, Object>();

        protected override void DrawBaseFields(SerializedProperty property, ref float y, Rect position, GUIContent label)
        {
            base.DrawBaseFields(property, ref y, position, label);
            DrawIfExists(property, ref y, position, "fromRotation");
            DrawIfExists(property, ref y, position, "toRotation");
            DrawIfExists(property, ref y, position, "target");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 1;
            extraLines += HasProperty(property, "fromRotation");
            extraLines += HasProperty(property, "toRotation");
            extraLines += HasProperty(property, "target");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }

        protected void TryAutoFillFromTarget(SerializedProperty property, SerializedProperty positionSpace = null)
        {
            var targetProp = property.FindPropertyRelative("target");
            var fromProp = property.FindPropertyRelative("fromRotation");
            if (targetProp == null || fromProp == null)
                return;

            string key = property.propertyPath; // unique per property
            Object currentTarget = targetProp.objectReferenceValue;

            // check if changed
            targetCache.TryGetValue(key, out Object previousTarget);
            if (previousTarget == currentTarget)
                return;

            // update cache
            targetCache[key] = currentTarget;
            if (currentTarget == null)
                return;

            Vector3 finalPos;
            if (currentTarget is Transform tr)
            {
                finalPos = tr.localEulerAngles;
               // finalPos = useLocal ? tr.localEulerAngles : tr.eulerAngles;
            }
            else
            {
                return;
            }
            fromProp.vector3Value = finalPos;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(RotateClip), true)]
    public class RotateClipDrawer : RotateClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Rotation";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyBeginCheck(position, property, label);
            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);

            // draw RotateClip fields via inherited helper
            DrawIfExists(property, ref y, position, "spaceType");
            TryAutoFillFromTarget(property);
            PropertyEndCheck(property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 0;
            extraLines += HasProperty(property, "spaceType");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }
}
#endif
