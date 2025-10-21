#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(PositionClipBase), true)]
    public class PositionClipBaseDrawer : TweenClipBaseDrawer
    {
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

                    switch ((Axis3D)transformAxis.enumValueIndex)
                    {
                        case Axis3D.X:
                            DrawIfExists(property, ref y, position, "toPosition.x");
                            break;
                        case Axis3D.Y:
                            DrawIfExists(property, ref y, position, "toPosition.y");
                            break;
                        case Axis3D.Z:
                            DrawIfExists(property, ref y, position, "toPosition.z");
                            break;
                        default:
                            DrawIfExists(property, ref y, position, "toPosition");
                            break;
                    }
                }
            }

            DrawIfExists(property, ref y, position, "target");
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
                if (rectAxis.enumValueIndex == (int)Axis2D.X)
                {
                    DrawIfExists(property, ref y, position, "toAnchoredPosition.x");
                }
                else if (rectAxis.enumValueIndex == (int)Axis2D.Y)
                {
                    DrawIfExists(property, ref y, position, "toAnchoredPosition.y");
                }
                else
                {
                    DrawIfExists(property, ref y, position, "toAnchoredPosition");
                }
            }
            DrawIfExists(property, ref y, position, "target");
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
