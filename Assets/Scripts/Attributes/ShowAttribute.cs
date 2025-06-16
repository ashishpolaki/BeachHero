using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    public class ShowAttribute : PropertyAttribute
    {
        public string BoolFieldName { get; }
        public ShowAttribute(string boolFieldName)
        {
            BoolFieldName = boolFieldName;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ShowAttribute))]
    public class ShowAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { if (!ShouldShow(property)) return; EditorGUI.PropertyField(position, property, label, true); }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShouldShow(property) ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
        }

        private bool ShouldShow(SerializedProperty property)
        {
            ShowAttribute showAttribute = (ShowAttribute)attribute;
            string boolFieldName = showAttribute.BoolFieldName;

            string propertyPath = property.propertyPath;
            string parentPath = propertyPath.Substring(0, propertyPath.LastIndexOf('.'));
            string boolPropPath = string.IsNullOrEmpty(parentPath) ? boolFieldName : parentPath + "." + boolFieldName;

            SerializedProperty boolProp = property.serializedObject.FindProperty(boolPropPath);

            return boolProp != null && boolProp.propertyType == SerializedPropertyType.Boolean && boolProp.boolValue;
        }
    }
#endif
}
