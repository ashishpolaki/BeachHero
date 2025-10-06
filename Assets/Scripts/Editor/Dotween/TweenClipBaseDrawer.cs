using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace BeachHero
{
    [CustomPropertyDrawer(typeof(TweenClipBase), true)]
    public class TweenClipBaseDrawer : PropertyDrawer
    {
        protected const float LINE_SPACING = 2f;

        // Draw the header + common TweenClipBase fields.
        protected virtual void DrawBaseFields(SerializedProperty property, ref float y, Rect position, GUIContent label)
        {
            float lineH = EditorGUIUtility.singleLineHeight;
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true

            };

            // Header
            var headerRect = new Rect(position.x, y, position.width, lineH);
            EditorGUI.LabelField(headerRect, $"Clip     : {HeaderLabel()}", labelStyle);
            y += lineH + LINE_SPACING;

            // Target Name
            var targetRect = new Rect(position.x, y, position.width, lineH);
            EditorGUI.LabelField(targetRect, $"Target : {TargetLabel(property)}", labelStyle);
            y += lineH + LINE_SPACING;

            DrawIfExists(property, ref y, position, "startTime");
            DrawIfExists(property, ref y, position, "duration");
            DrawIfExists(property, ref y, position, "ease");
            DrawIfExists(property, ref y, position, "snapping");
        }

        protected virtual string HeaderLabel()
        {
            return string.Empty;
        }

        protected virtual string TargetLabel(SerializedProperty property)
        {
            return "<color=#FF4040>Target Null</color>";
        }

        // Helper that advances y (no lambdas capturing ref)
        protected void DrawIfExists(SerializedProperty property, ref float y, Rect position, string propName)
        {
            var p = property.FindPropertyRelative(propName);
            if (p != null)
            {
                float lineH = EditorGUIUtility.singleLineHeight;
                var r = new Rect(position.x, y, position.width, lineH);
                EditorGUI.PropertyField(r, p);
                y += lineH + LINE_SPACING;
            }
        }

        protected float GetBaseHeight(SerializedProperty property)
        {
            int lines = 1;
            System.Func<string, int> Exists = (name) => property.FindPropertyRelative(name) != null ? 1 : 0;

            lines += Exists("startTime");
            lines += Exists("duration");
            lines += Exists("ease");
            lines += Exists("snapping");

            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return lines * singleLineTotal;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();
            float y = position.y;
            DrawBaseFields(property, ref y, position, label);
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetBaseHeight(property);
        }
        public virtual void DrawCaptureFromButton(Rect position, float y, SerializedProperty property)
        {
            // Capture From button
            float lineH = EditorGUIUtility.singleLineHeight;
            var btnRect = new Rect(position.x, y, position.width, lineH);
            if (GUI.Button(btnRect, "Capture From"))
            {
                var clipObj = property.managedReferenceValue;
                if (clipObj != null)
                {
                    var owner = property.serializedObject.targetObject;
                    if (owner != null) Undo.RecordObject(owner, "Capture From Clip");

                    var mi = clipObj.GetType().GetMethod("CaptureFromState", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (mi != null) mi.Invoke(clipObj, null);
                    else
                    {
                        var f = clipObj.GetType().GetField("fromPosition");
                        var tf = clipObj.GetType().GetField("target");
                        if (f != null && tf != null)
                        {
                            var tVal = tf.GetValue(clipObj) as Transform;
                            if (tVal != null) f.SetValue(clipObj, tVal.position);
                        }
                    }

                    property.serializedObject.ApplyModifiedProperties();
                    if (owner != null)
                    {
                        EditorUtility.SetDirty(owner);
                        var go = (owner as Component)?.gameObject;
                        if (go != null) EditorSceneManager.MarkSceneDirty(go.scene);
                    }
                }
            }
        }
    }

    #region Move

    [CustomPropertyDrawer(typeof(MoveClipBase), true)]
    public class MoveClipBaseDrawer : TweenClipBaseDrawer
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
            System.Func<string, int> Exists = (n) => property.FindPropertyRelative(n) != null ? 1 : 0;
            extraLines += Exists("fromPosition");

            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }

    [CustomPropertyDrawer(typeof(TransformMoveClip), true)]
    public class TransformMoveClipDrawer : MoveClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Transform Move";
        }

        protected override string TargetLabel(SerializedProperty property)
        {
            var targetProp = property.FindPropertyRelative("target");

            if (targetProp != null && targetProp.objectReferenceValue != null)
            {
                return targetProp.objectReferenceValue.name;
            }
            //Find Target Property 
            return base.TargetLabel(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.serializedObject.Update();

            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);

            // draw MoveClipBase fields via inherited helper
            var positionSpace = property.FindPropertyRelative("positionSpace");
            if (positionSpace != null)
            {
                DrawIfExists(property, ref y, position, "positionSpace");
                if (positionSpace.enumValueIndex == (int)SpaceType.World)
                {
                    var transformAxis = property.FindPropertyRelative("transformAxis");
                    if (transformAxis != null)
                    {
                        DrawIfExists(property, ref y, position, "transformAxis");
                        if (transformAxis.enumValueIndex == (int)Axis3D.X)
                        {
                            DrawIfExists(property, ref y, position, "toPosition.x");
                        }
                        else if (transformAxis.enumValueIndex == (int)Axis3D.Y)
                        {
                            DrawIfExists(property, ref y, position, "toPosition.y");
                        }
                        else if (transformAxis.enumValueIndex == (int)Axis3D.Z)
                        {
                            DrawIfExists(property, ref y, position, "toPosition.z");
                        }
                        else
                        {
                            DrawIfExists(property, ref y, position, "toPosition");
                        }
                    }
                }
                else if (positionSpace.enumValueIndex == (int)SpaceType.Local)
                {
                    var transformAxis = property.FindPropertyRelative("transformAxis");
                    if (transformAxis != null)
                    {
                        DrawIfExists(property, ref y, position, "transformAxis");
                        if (transformAxis.enumValueIndex == (int)Axis3D.X)
                        {
                            DrawIfExists(property, ref y, position, "toPosition.x");
                        }
                        else if (transformAxis.enumValueIndex == (int)Axis3D.Y)
                        {
                            DrawIfExists(property, ref y, position, "toPosition.y");
                        }
                        else if (transformAxis.enumValueIndex == (int)Axis3D.Z)
                        {
                            DrawIfExists(property, ref y, position, "toPosition.z");
                        }
                        else
                        {
                            DrawIfExists(property, ref y, position, "toPosition");
                        }
                    }
                }
            }
            DrawIfExists(property, ref y, position, "target");
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);

            int extraLines = 0;
            System.Func<string, int> Exists = (n) => property.FindPropertyRelative(n) != null ? 1 : 0;
            extraLines += Exists("positionSpace");
            extraLines += Exists("target");
            extraLines += Exists("toPosition");
            extraLines += Exists("transformAxis");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }

    [CustomPropertyDrawer(typeof(RectTransformMoveClip), true)]
    public class RectTransformMoveClipDrawer : MoveClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Rect Anchor Move";
        }

        protected override string TargetLabel(SerializedProperty property)
        {
            var targetProp = property.FindPropertyRelative("target");

            if (targetProp != null && targetProp.objectReferenceValue != null)
            {
                return targetProp.objectReferenceValue.name;
            }
            return base.TargetLabel(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.serializedObject.Update();
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

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);

            int extraLines = 0;
            System.Func<string, int> Exists = (n) => property.FindPropertyRelative(n) != null ? 1 : 0;
            extraLines += Exists("target");
            extraLines += Exists("toAnchoredPosition");
            extraLines += Exists("rectAxis");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }
    #endregion

    #region Scale

    [CustomPropertyDrawer(typeof(ScaleClip), true)]
    public class ScaleClipBaseDrawer : TweenClipBaseDrawer
    {
        protected override void DrawBaseFields(SerializedProperty property, ref float y, Rect position, GUIContent label)
        {
            base.DrawBaseFields(property, ref y, position, label);
            DrawIfExists(property, ref y, position, "fromScale");
            DrawIfExists(property, ref y, position, "toScale");
            DrawIfExists(property, ref y, position, "target");
        }

        protected override string HeaderLabel()
        {
            return "Scale";
        }

        protected override string TargetLabel(SerializedProperty property)
        {
            var targetProp = property.FindPropertyRelative("target");

            if (targetProp != null && targetProp.objectReferenceValue != null)
            {
                return targetProp.objectReferenceValue.name;
            }
            //Find Target Property 
            return base.TargetLabel(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.serializedObject.Update();
            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);

            int extraLines = 1;
            System.Func<string, int> Exists = (n) => property.FindPropertyRelative(n) != null ? 1 : 0;
            extraLines += Exists("target");
            extraLines += Exists("fromScale");
            extraLines += Exists("toScale");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }

    [CustomPropertyDrawer(typeof(PunchScaleClip), true)]
    public class PunchScaleClipDrawer : ScaleClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Punch Scale";
        }

        protected override string TargetLabel(SerializedProperty property)
        {
            var targetProp = property.FindPropertyRelative("target");

            if (targetProp != null && targetProp.objectReferenceValue != null)
            {
                return targetProp.objectReferenceValue.name;
            }
            //Find Target Property 
            return base.TargetLabel(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.serializedObject.Update();
            float y = position.y;

            // draw base part
            DrawBaseFields(property, ref y, position, label);
            DrawIfExists(property, ref y, position, "vibrato");
            DrawIfExists(property, ref y, position, "elasticity");

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);

            int extraLines = 0;
            System.Func<string, int> Exists = (n) => property.FindPropertyRelative(n) != null ? 1 : 0;
            extraLines += Exists("vibrato");
            extraLines += Exists("elasticity");
            float singleLineTotal = EditorGUIUtility.singleLineHeight + LINE_SPACING;
            return baseHeight + extraLines * singleLineTotal;
        }
    }


    [CustomPropertyDrawer(typeof(BlendableScaleClip), true)]
    public class BlendableScaleClipDrawer : ScaleClipBaseDrawer
    {
        protected override string HeaderLabel()
        {
            return "Blendable Scale";
        }
    }

    #endregion
}
