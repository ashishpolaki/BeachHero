using UnityEngine;
using UnityEditor;
using System.IO;

namespace Watermelon
{
    [CustomEditor(typeof(CurrencyInitModule))]
    public class CurrencyInitModuleEditor : InitModuleEditor
    {
        public override void OnCreated()
        {
            CurrenciesDatabase currenciesDatabase = EditorUtils.GetAsset<CurrenciesDatabase>();
            if (currenciesDatabase == null)
            {
                currenciesDatabase = (CurrenciesDatabase)ScriptableObject.CreateInstance<CurrenciesDatabase>();
                currenciesDatabase.name = "Currencies Database";

                string referencePath = AssetDatabase.GetAssetPath(target);
                string directoryPath = Path.GetDirectoryName(referencePath);

                // Create a unique file path for the ScriptableObject
                string assetPath = Path.Combine(directoryPath, currenciesDatabase.name + ".asset");
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

                // Save the ScriptableObject to the determined path
                AssetDatabase.CreateAsset(currenciesDatabase, assetPath);
                AssetDatabase.SaveAssets();

                EditorUtility.SetDirty(target);
            }

            serializedObject.Update();
            serializedObject.FindProperty("currenciesDatabase").objectReferenceValue = currenciesDatabase;
            serializedObject.ApplyModifiedProperties();
        }
    }
}