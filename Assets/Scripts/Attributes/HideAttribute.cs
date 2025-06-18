using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    public class HideAttribute : PropertyAttribute
    {
        public string BoolFieldName { get; }

        public HideAttribute(string boolFieldName)
        {
            BoolFieldName = boolFieldName;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HideAttribute))]
    public class HideAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldHide(property)) return;
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShouldHide(property) ? 0f : EditorGUI.GetPropertyHeight(property, label, true);
        }

        private bool ShouldHide(SerializedProperty property)
        {
            if (property == null || property.serializedObject == null)
            {
                return true; // Hide if property or serializedObject is null
            }

            HideAttribute hideAttribute = (HideAttribute)attribute;
            string boolFieldName = hideAttribute.BoolFieldName;

            string propertyPath = property.propertyPath;
            int lastDot = propertyPath.LastIndexOf('.');
            string parentPath = lastDot == -1 ? string.Empty : propertyPath.Substring(0, lastDot);
            string boolPropPath = string.IsNullOrEmpty(parentPath) ? boolFieldName : parentPath + "." + boolFieldName;

            SerializedProperty boolProp = property.serializedObject.FindProperty(boolPropPath);

            return boolProp != null && boolProp.propertyType == SerializedPropertyType.Boolean && boolProp.boolValue;
        }
    }
#endif
}
