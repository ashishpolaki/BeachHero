using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomEditor(typeof(AbstractSkinsDatabase), true)]
    public class AbstractSkinsProviderEditor : CustomInspector
    {
        private SkinsController skinsController;
        private bool isRegistered;

        protected override void OnEnable()
        {
            base.OnEnable();

            AbstractSkinsDatabase database = (AbstractSkinsDatabase)target;

            EditorSkinsProvider.AddDatabase(database);

            skinsController = GameObject.FindObjectOfType<SkinsController>();
            if(skinsController != null && skinsController.Handler != null)
            {
                isRegistered = skinsController.Handler.HasSkinsProvider(database);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(skinsController != null)
            {
                if (!isRegistered)
                {
                    GUILayout.Space(12);

                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.HelpBox("This database isn't linked to SkinsController", MessageType.Warning);

                    if (GUILayout.Button("Add to Skins Handler"))
                    {
                        SerializedObject skinsHandlerSerializedObject = new SerializedObject(skinsController);

                        skinsHandlerSerializedObject.Update();

                        SerializedProperty handlerProperty = skinsHandlerSerializedObject.FindProperty("handler");

                        SerializedProperty providersProperty = handlerProperty.FindPropertyRelative("skinProviders");
                        int index = providersProperty.arraySize;

                        providersProperty.arraySize = index + 1;

                        SerializedProperty providerProperty = providersProperty.GetArrayElementAtIndex(index);
                        providerProperty.objectReferenceValue = target;

                        skinsHandlerSerializedObject.ApplyModifiedProperties();

                        isRegistered = true;
                    }

                    EditorGUILayout.EndVertical();
                }
            }
        }
    }
}
