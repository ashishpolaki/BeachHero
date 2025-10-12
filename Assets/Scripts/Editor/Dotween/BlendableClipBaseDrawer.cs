using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(BlendableClipBase), true)]
    public class BlendableClipBaseDrawer : TweenClipBaseDrawer
    {
        protected override void DrawBaseFields(SerializedProperty property, ref float y, Rect position, GUIContent label)
        {
            base.DrawBaseFields(property, ref y, position, label);
            DrawIfExists(property, ref y, position, "target");
            DrawIfExists(property, ref y, position, "byValue");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 1;
            extraLines += HasProperty(property, "target");
            extraLines += HasProperty(property, "byValue");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }

    [CustomPropertyDrawer(typeof(BlendablePositionClip), true)]
    public class BlendablePositionClipDrawer : BlendableClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Blendable Position";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyBeginCheck(position, property, label);
            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);

            // draw BlendablePositionClip fields via inherited helper
            var spaceType = property.FindPropertyRelative("spaceType");
            if (spaceType != null)
            {
                DrawIfExists(property, ref y, position, "spaceType");
            }

            PropertyEndCheck(property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 0;
            extraLines += HasProperty(property, "spaceType");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }

    [CustomPropertyDrawer(typeof(BlendableRotationClip), true)]
    public class BlendableRotationClipDrawer : BlendableClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Blendable Rotation";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.serializedObject.Update();

            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);

            // draw BlendableRotationClip fields via inherited helper
            var spaceType = property.FindPropertyRelative("spaceType");
            if (spaceType != null)
            {
                DrawIfExists(property, ref y, position, "spaceType");
            }
            var rotateMode = property.FindPropertyRelative("rotateMode");
            if (rotateMode != null)
            {
                DrawIfExists(property, ref y, position, "rotateMode");
            }

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 0;
            extraLines += HasProperty(property, "spaceType");
            extraLines += HasProperty(property, "rotateMode");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }

    [CustomPropertyDrawer(typeof(BlendableScaleClip), true)]
    public class BlendableScaleClipDrawer : BlendableClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Blendable Scale";
        }
    }

    [CustomPropertyDrawer(typeof(BlendablePunchRotationClip), true)]
    public class BlendablePunchRotationClipDrawer : BlendableClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Blendable Punch Rotation";
        }

        protected override void DrawBaseFields(SerializedProperty property, ref float y, Rect position, GUIContent label)
        {
            base.DrawBaseFields(property, ref y, position, label);
            DrawIfExists(property, ref y, position, "vibrato");
            DrawIfExists(property, ref y, position, "elasticity");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);
            int extraLines = 0;
            extraLines += HasProperty(property, "vibrato");
            extraLines += HasProperty(property, "elasticity");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }
}
