#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(CanvasGroupClipBase), true)]
    public class CanvasGroupClipBaseDrawer : TweenClipBaseDrawer
    {
        protected override void DrawBaseFields(SerializedProperty property, ref float y, Rect position, GUIContent label)
        {
            base.DrawBaseFields(property, ref y, position, label);
            DrawIfExists(property, ref y, position, "target");
            DrawIfExists(property, ref y, position, "fromAlpha");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 1;
            extraLines += HasProperty(property, "target");
            extraLines += HasProperty(property, "fromAlpha");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }

    [CustomPropertyDrawer(typeof(CanvasGroupFadeClip), true)]
    public class CanvasGroupFadeClipDrawer : CanvasGroupClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "CanvasGroup Fade";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyBeginCheck(position, property, label);
            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);

            // draw CanvasGroupFadeClip fields via inherited helper
            DrawIfExists(property, ref y, position, "toAlpha");

            PropertyEndCheck(property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 1;
            extraLines += HasProperty(property, "toAlpha");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }
}
#endif
