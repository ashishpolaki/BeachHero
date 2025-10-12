using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(ScaleClipBase), true)]
    public class ScaleClipBaseDrawer : TweenClipBaseDrawer
    {
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
            switch ((Axis3D)scaleAxisProp.enumValueIndex)
            {
                case Axis3D.X:
                    DrawIfExists(property, ref y, position, "toScale.x");
                    break;
                case Axis3D.Y:
                    DrawIfExists(property, ref y, position, "toScale.y");
                    break;
                case Axis3D.Z:
                    DrawIfExists(property, ref y, position, "toScale.z");
                    break;
                case Axis3D.XYZ:
                    DrawIfExists(property, ref y, position, "toScale");
                    break;
            }

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