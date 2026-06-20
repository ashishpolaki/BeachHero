#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(ShakeClipBase), true)]
    public class ShakeClipBaseDrawer : TweenClipBaseDrawer
    {
        protected static Dictionary<string, Object> targetCache = new Dictionary<string, Object>();

        protected override void DrawBaseFields(SerializedProperty property, ref float y, Rect position, GUIContent label)
        {
            base.DrawBaseFields(property, ref y, position, label);
            DrawIfExists(property, ref y, position, "target");
            DrawIfExists(property, ref y, position, "originalValue");
            DrawIfExists(property, ref y, position, "startValue");
            DrawIfExists(property, ref y, position, "strength");
            DrawIfExists(property, ref y, position, "frequency");
            DrawIfExists(property, ref y, position, "dampingRatio");
            DrawIfExists(property, ref y, position, "randomSeed");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyBeginCheck(position, property, label);
            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);
            TryAutoFillFromTarget(property);
            PropertyEndCheck(property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 1;
            extraLines += HasProperty(property, "target");
            extraLines += HasProperty(property, "originalValue");
            extraLines += HasProperty(property, "startValue");
            extraLines += HasProperty(property, "strength");
            extraLines += HasProperty(property, "frequency");
            extraLines += HasProperty(property, "dampingRatio");
            extraLines += HasProperty(property, "randomSeed");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
        protected virtual bool TryGetValueFromTarget(Transform tr, out Vector3 value)
        {
            value = default;
            return false;
        }
        protected void TryAutoFillFromTarget(SerializedProperty property)
        {
            var targetProp = property.FindPropertyRelative("target");
            var fromProp = property.FindPropertyRelative("startValue");
            var OrigProp = property.FindPropertyRelative("originalValue");

            if (targetProp == null || fromProp == null)
                return;

            string key = property.propertyPath;
            Object currentTarget = targetProp.objectReferenceValue;

            targetCache.TryGetValue(key, out Object previousTarget);

            // Only run when target changes
            if (previousTarget == currentTarget)
                return;

            targetCache[key] = currentTarget;

            if (currentTarget is not Transform tr)
                return;

            if (!TryGetValueFromTarget(tr, out Vector3 value))
                return;

            fromProp.vector3Value = value;
            OrigProp.vector3Value = value;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(ShakePositionClip), true)]
    public class ShakePositionClipDrawer : ShakeClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Shake Position";
        }
        protected override bool TryGetValueFromTarget(Transform tr, out Vector3 value)
        {
            value = tr.localPosition;
            return true;
        }
    }

    [CustomPropertyDrawer(typeof(ShakeScaleClip), true)]
    public class ShakeScaleClipDrawer : ShakeClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Shake Scale";
        }
        protected override bool TryGetValueFromTarget(Transform tr, out Vector3 value)
        {
            value = tr.localScale;
            return true;
        }
    }
}
#endif
