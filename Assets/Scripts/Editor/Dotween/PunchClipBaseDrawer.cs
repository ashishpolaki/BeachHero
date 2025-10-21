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
            DrawIfExists(property, ref y, position, "punch");
            DrawIfExists(property, ref y, position, "vibrato");
            DrawIfExists(property, ref y, position, "elasticity");
            DrawIfExists(property, ref y, position, "originalScale");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 1;
            extraLines += HasProperty(property, "target");
            extraLines += HasProperty(property, "punch");
            extraLines += HasProperty(property, "vibrato");
            extraLines += HasProperty(property, "elasticity");
            extraLines += HasProperty(property, "originalScale");
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

    [CustomPropertyDrawer(typeof(PunchRotationClip), true)]
    public class PunchRotationClipDrawer : PunchClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Punch Rotation";
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

    [CustomPropertyDrawer(typeof(PunchAnchorPosClip), true)]
    public class PunchAnchorPosClipDrawer : PunchClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Punch Anchor Pos";
        }
    }
}