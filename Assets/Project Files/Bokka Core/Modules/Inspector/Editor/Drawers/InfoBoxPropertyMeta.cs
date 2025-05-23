﻿using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Bokka
{
    public class InfoBoxPropertyMeta
    {
        public void ApplyPropertyMeta(SerializedProperty property)
        {
            //InfoBoxAttribute infoBoxAttribute = (InfoBoxAttribute)metaAttribute;
            InfoBoxAttribute infoBoxAttribute = new InfoBoxAttribute("test", "test");
            UnityEngine.Object target = PropertyUtility.GetTargetObject(property);

            if (!string.IsNullOrEmpty(infoBoxAttribute.VisibleIf))
            {
                FieldInfo conditionField = target.GetType().GetField(infoBoxAttribute.VisibleIf, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (conditionField != null &&
                    conditionField.FieldType == typeof(bool))
                {
                    if ((bool)conditionField.GetValue(target))
                    {
                        DrawInfoBox(infoBoxAttribute.Text, infoBoxAttribute.Type);
                    }

                    return;
                }

                MethodInfo conditionMethod = target.GetType().GetMethod(infoBoxAttribute.VisibleIf, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (conditionMethod != null &&
                    conditionMethod.ReturnType == typeof(bool) &&
                    conditionMethod.GetParameters().Length == 0)
                {
                    if ((bool)conditionMethod.Invoke(target, null))
                    {
                        DrawInfoBox(infoBoxAttribute.Text, infoBoxAttribute.Type);
                    }

                    return;
                }

                string warning = infoBoxAttribute.GetType().Name + " needs a valid boolean condition field or method name to work";
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
                Debug.LogWarning(warning, PropertyUtility.GetTargetObject(property));
            }
            else
            {
                DrawInfoBox(infoBoxAttribute.Text, infoBoxAttribute.Type);
            }
        }

        private void DrawInfoBox(string infoText, InfoBoxType infoBoxType)
        {
            switch (infoBoxType)
            {
                case InfoBoxType.Normal:
                    EditorGUILayout.HelpBox(infoText, MessageType.Info);
                    break;

                case InfoBoxType.Warning:
                    EditorGUILayout.HelpBox(infoText, MessageType.Warning);
                    break;

                case InfoBoxType.Error:
                    EditorGUILayout.HelpBox(infoText, MessageType.Error);
                    break;
            }
        }
    }
}