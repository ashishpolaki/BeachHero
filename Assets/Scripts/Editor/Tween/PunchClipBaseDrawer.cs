#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(PunchClipBase), true)]
    public class PunchClipBaseDrawer : TweenClipBaseDrawer
    {
        protected override void DrawBaseFields(SerializedProperty property, ref float y, Rect position, GUIContent label)
        {
            base.DrawBaseFields(property, ref y, position, label);
            DrawIfExists(property, ref y, position, "target");
            DrawIfExists(property, ref y, position, "originalScale");
            DrawIfExists(property, ref y, position, "startValue");
            DrawIfExists(property, ref y, position, "strength");
            DrawIfExists(property, ref y, position, "transformSpace");
            DrawIfExists(property, ref y, position, "frequency");
            DrawIfExists(property, ref y, position, "damper");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 1;
            extraLines += HasProperty(property, "target");
            extraLines += HasProperty(property, "originalScale");
            extraLines += HasProperty(property, "startValue");
            extraLines += HasProperty(property, "strength");
            extraLines += HasProperty(property, "transformSpace");
            extraLines += HasProperty(property, "frequency");
            extraLines += HasProperty(property, "damper");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyBeginCheck(position, property, label);
            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);

            PropertyEndCheck(property);
        }
    }

    [CustomPropertyDrawer(typeof(PunchPositionClip), true)]
    public class PunchPositionClipDrawer : PunchClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Punch Position";
        }
    }

    [CustomPropertyDrawer(typeof(PunchScaleClip), true)]
    public class PunchScaleClipDrawer : PunchClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Punch Scale";
        }
    }
}
#endif
