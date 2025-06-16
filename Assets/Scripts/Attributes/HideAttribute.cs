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
            HideAttribute hideAttribute = (HideAttribute)attribute;
            string boolFieldName = hideAttribute.BoolFieldName;

            // Get the path to the controlling bool relative to the current property
            string propertyPath = property.propertyPath;
            string parentPath = propertyPath.Substring(0, propertyPath.LastIndexOf('.'));
            string boolPropPath = string.IsNullOrEmpty(parentPath) ? boolFieldName : parentPath + "." + boolFieldName;

            SerializedProperty boolProp = property.serializedObject.FindProperty(boolPropPath);

            return boolProp != null && boolProp.propertyType == SerializedPropertyType.Boolean && boolProp.boolValue;
        }
    }
#endif
}
