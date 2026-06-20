#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(ScaleClipBase), true)]
    public class ScaleClipBaseDrawer : TweenClipBaseDrawer
    {
        static Dictionary<string, Object> targetCache = new Dictionary<string, Object>();

        protected override void DrawBaseFields(SerializedProperty property, ref float y, Rect position, GUIContent label)
        {
            base.DrawBaseFields(property, ref y, position, label);
            DrawIfExists(property, ref y, position, "fromScale");
            DrawIfExists(property, ref y, position, "target");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 1;
            extraLines += HasProperty(property, "fromScale");
            extraLines += HasProperty(property, "target");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }

        protected void TryAutoFillFromTarget(SerializedProperty property, SerializedProperty positionSpace = null)
        {
            var targetProp = property.FindPropertyRelative("target");
            var fromProp = property.FindPropertyRelative("fromScale");
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
                finalPos = tr.localScale;
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

    [CustomPropertyDrawer(typeof(ScaleClip), true)]
    public class ScaleClipDrawer : ScaleClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Scale";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyBeginCheck(position, property, label);
            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);

            // draw ScaleClip fields via inherited helper
            var scaleAxisProp = property.FindPropertyRelative("scaleAxis");
            DrawIfExists(property, ref y, position, "scaleAxis");
            switch ((TransformAxis)scaleAxisProp.enumValueIndex)
            {
                case TransformAxis.X:
                    DrawIfExists(property, ref y, position, "toScale.x");
                    break;
                case TransformAxis.Y:
                    DrawIfExists(property, ref y, position, "toScale.y");
                    break;
                case TransformAxis.Z:
                    DrawIfExists(property, ref y, position, "toScale.z");
                    break;
                case TransformAxis.XYZ:
                    DrawIfExists(property, ref y, position, "toScale");
                    break;
            }
            TryAutoFillFromTarget(property);
            PropertyEndCheck(property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 0;
            extraLines += HasProperty(property, "scaleAxis");
            extraLines += HasProperty(property, "toScale");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }


}
#endif
