#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(SizeDeltaClipBase), true)]
    public class SizeDeltaClipBaseDrawer : TweenClipBaseDrawer
    {
        static Dictionary<string, Object> targetCache = new Dictionary<string, Object>();
        protected void TryAutoFillFromTarget(SerializedProperty property)
        {
            var targetProp = property.FindPropertyRelative("target");
            var fromProp = property.FindPropertyRelative("fromSizeDelta");
            if (targetProp == null || fromProp == null)
                return;

            string key = property.propertyPath;
            Object currentTarget = targetProp.objectReferenceValue;

            targetCache.TryGetValue(key, out Object previousTarget);
            if (previousTarget == currentTarget)
                return;

            targetCache[key] = currentTarget;
            if (currentTarget == null)
                return;

            if (currentTarget is RectTransform rt)
            {
                fromProp.vector2Value = rt.sizeDelta;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }

    [CustomPropertyDrawer(typeof(SizeDeltaClip), true)]
    public class SizeDeltaClipDrawer : SizeDeltaClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Size Delta";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyBeginCheck(position, property, label);
            float y = position.y;

            // Draw base fields (startTime, duration, ease, etc.)
            DrawBaseFields(property, ref y, position, label);

            // rectAxis
            var rectAxis = property.FindPropertyRelative("rectAxis");
            DrawIfExists(property, ref y, position, "rectAxis");

            TransformAxis axis = rectAxis != null ? (TransformAxis)rectAxis.enumValueIndex : TransformAxis.XY;

            // Dynamic From size field
            switch (axis)
            {
                case TransformAxis.X:
                    DrawIfExists(property, ref y, position, "fromSizeDelta.x", "From Width (X)");
                    break;
                case TransformAxis.Y:
                    DrawIfExists(property, ref y, position, "fromSizeDelta.y", "From Height (Y)");
                    break;
                case TransformAxis.XY:
                default:
                    DrawIfExists(property, ref y, position, "fromSizeDelta", "From Size Delta");
                    break;
            }

            // Dynamic To size field
            switch (axis)
            {
                case TransformAxis.X:
                    DrawIfExists(property, ref y, position, "toSizeDelta.x", "To Width (X)");
                    break;
                case TransformAxis.Y:
                    DrawIfExists(property, ref y, position, "toSizeDelta.y", "To Height (Y)");
                    break;
                case TransformAxis.XY:
                default:
                    DrawIfExists(property, ref y, position, "toSizeDelta", "To Size Delta");
                    break;
            }

            DrawIfExists(property, ref y, position, "target");
            TryAutoFillFromTarget(property);
            PropertyEndCheck(property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);

            int extraLines = 1;
            extraLines += HasProperty(property, "rectAxis");
            extraLines += HasProperty(property, "fromSizeDelta");
            extraLines += HasProperty(property, "toSizeDelta");
            extraLines += HasProperty(property, "target");

            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }
}
#endif
