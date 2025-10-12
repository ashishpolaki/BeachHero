#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using DG.Tweening;
using UnityEngine.UIElements;

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

            //If ease is Back, show overshoot
            var easeProp = property.FindPropertyRelative("ease");
            if (easeProp != null)
            {
                if (easeProp.enumValueIndex == (int)Ease.InBack || easeProp.enumValueIndex == (int)Ease.OutBack || easeProp.enumValueIndex == (int)Ease.InOutBack)
                {
                    DrawIfExists(property, ref y, position, "overshoot");
                }
                else if (easeProp.enumValueIndex == (int)Ease.InElastic || easeProp.enumValueIndex == (int)Ease.OutElastic || easeProp.enumValueIndex == (int)Ease.InOutElastic)
                {
                    DrawIfExists(property, ref y, position, "amplitude");
                    DrawIfExists(property, ref y, position, "period");
                }
                else if (easeProp.enumValueIndex == (int)Ease.InFlash || easeProp.enumValueIndex == (int)Ease.OutFlash || easeProp.enumValueIndex == (int)Ease.InOutFlash)
                {
                    DrawIfExists(property, ref y, position, "amplitude", "Flash Count");
                    DrawIfExists(property, ref y, position, "period", "Flash Duration");
                }
            }
        }

        protected virtual string HeaderLabel()
        {
            return string.Empty;
        }

        protected int HasProperty(SerializedProperty parent, string path)
        {
            return parent.FindPropertyRelative(path) != null ? 1 : 0;
        }
        private string TargetLabel(SerializedProperty property)
        {
            var targetProp = property.FindPropertyRelative("target");

            if (targetProp != null && targetProp.objectReferenceValue != null)
            {
                return targetProp.objectReferenceValue.name;
            }
            return "<color=#FF4040>Target Null</color>";
        }

        protected void PropertyBeginCheck(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.serializedObject.Update();
            EditorGUI.BeginChangeCheck();
        }

        protected void PropertyEndCheck(SerializedProperty property)
        {
            if (EditorGUI.EndChangeCheck())
            {
                TweenSequencerEditor.Instance.OnClipOrSequencerDataChanged();
            }
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        protected void DrawIfExists(SerializedProperty parent, ref float y, Rect position, string propName, string labelOverride = null)
        {
            SerializedProperty prop = parent.FindPropertyRelative(propName);
            if (prop != null)
            {
                Rect fieldRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(fieldRect, prop, new GUIContent(labelOverride ?? prop.displayName));
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
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

            var easeProp = property.FindPropertyRelative("ease");
            if (easeProp != null)
            {
                if (easeProp.enumValueIndex == (int)Ease.InBack || easeProp.enumValueIndex == (int)Ease.OutBack || easeProp.enumValueIndex == (int)Ease.InOutBack)
                {
                    lines += Exists("overshoot");
                }
                else if (easeProp.enumValueIndex == (int)Ease.InElastic || easeProp.enumValueIndex == (int)Ease.OutElastic || easeProp.enumValueIndex == (int)Ease.InOutElastic ||
                    easeProp.enumValueIndex == (int)Ease.InFlash || easeProp.enumValueIndex == (int)Ease.OutFlash || easeProp.enumValueIndex == (int)Ease.InOutFlash)
                {
                    lines += Exists("amplitude");
                    lines += Exists("period");
                }
            }

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
}
#endif
