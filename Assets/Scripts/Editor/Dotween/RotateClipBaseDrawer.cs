#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(RotateClipBase), true)]
    public class RotateClipBaseDrawer : TweenClipBaseDrawer
    {
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
            DrawIfExists(property, ref y, position, "rotateMode");
            DrawIfExists(property, ref y, position, "spaceType");

            PropertyEndCheck(property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 0;
            extraLines += HasProperty(property, "rotateMode");
            extraLines += HasProperty(property, "spaceType");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }
}
#endif
