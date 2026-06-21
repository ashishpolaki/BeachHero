#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(PositionClipBase), true)]
    public class PositionClipBaseDrawer : TweenClipBaseDrawer
    {
        static Dictionary<string, Object> targetCache = new Dictionary<string, Object>();

        protected override void DrawBaseFields(SerializedProperty property, ref float y, Rect position, GUIContent label)
        {
            base.DrawBaseFields(property, ref y, position, label);
            DrawIfExists(property, ref y, position, "fromPosition");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 1;
            extraLines += HasProperty(property, "fromPosition");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }

        protected void TryAutoFillFromTarget(SerializedProperty property, SerializedProperty positionSpace = null)
        {
            var targetProp = property.FindPropertyRelative("target");
            var fromProp = property.FindPropertyRelative("fromPosition");
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
            //  Handle RectTransform
            if (currentTarget is RectTransform rt)
            {
                Vector2 anchored = rt.anchoredPosition;
                finalPos = new Vector3(anchored.x, anchored.y, 0f);
            }
            //  Handle Transform
            else if (currentTarget is Transform tr)
            {
                //bool useLocal = positionSpace != null &&
                //                positionSpace.enumValueIndex == (int)TransformSpace.Local;

                //finalPos = useLocal ? tr.localPosition : tr.position;
                finalPos = tr.localPosition;
            }
            else
            {
                return;
            }
            fromProp.vector3Value = finalPos;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(TransformPositionClip), true)]
    public class TransformPositionClipDrawer : PositionClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Transform Position";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyBeginCheck(position, property, label);
            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);

            // draw TransformPositionClip fields via inherited helper
            var positionSpace = property.FindPropertyRelative("positionSpace");
            if (positionSpace != null)
            {
                DrawIfExists(property, ref y, position, "positionSpace");
                var transformAxis = property.FindPropertyRelative("transformAxis");
                if (transformAxis != null)
                {
                    DrawIfExists(property, ref y, position, "transformAxis");

                    switch ((TransformAxis)transformAxis.enumValueIndex)
                    {
                        case TransformAxis.X:
                            DrawIfExists(property, ref y, position, "toPosition.x");
                            break;
                        case TransformAxis.Y:
                            DrawIfExists(property, ref y, position, "toPosition.y");
                            break;
                        case TransformAxis.Z:
                            DrawIfExists(property, ref y, position, "toPosition.z");
                            break;
                        default:
                            DrawIfExists(property, ref y, position, "toPosition");
                            break;
                    }
                }
            }

            DrawIfExists(property, ref y, position, "target");
            TryAutoFillFromTarget(property, positionSpace);
            PropertyEndCheck(property);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);

            int extraLines = 0;
            extraLines += HasProperty(property, "positionSpace");
            extraLines += HasProperty(property, "target");
            extraLines += HasProperty(property, "toPosition");
            extraLines += HasProperty(property, "transformAxis");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }

    [CustomPropertyDrawer(typeof(AnchorPositionClip), true)]
    public class AnchorPositionClipDrawer : PositionClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Anchor Position";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyBeginCheck(position, property, label);
            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);
            var rectAxis = property.FindPropertyRelative("rectAxis");
            if (rectAxis != null)
            {
                DrawIfExists(property, ref y, position, "rectAxis");
                if (rectAxis.enumValueIndex == (int)TransformAxis.X)
                {
                    DrawIfExists(property, ref y, position, "toAnchoredPosition.x");
                }
                else if (rectAxis.enumValueIndex == (int)TransformAxis.Y)
                {
                    DrawIfExists(property, ref y, position, "toAnchoredPosition.y");
                }
                else
                {
                    DrawIfExists(property, ref y, position, "toAnchoredPosition");
                }
            }
            DrawIfExists(property, ref y, position, "target");
            TryAutoFillFromTarget(property);
            PropertyEndCheck(property);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);

            int extraLines = 0;
            extraLines += HasProperty(property, "target");
            extraLines += HasProperty(property, "toAnchoredPosition");
            extraLines += HasProperty(property, "rectAxis");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }
}
#endif
