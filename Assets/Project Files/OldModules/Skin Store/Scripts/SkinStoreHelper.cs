using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Watermelon.SkinStore
{
    public static class SkinStoreHelper
    {
        public static void SetTabState(string tabName, bool state)
        {
            SkinStoreDatabase skinsDatabase = RuntimeEditorUtils.GetAssetByName<SkinStoreDatabase>();
            if (skinsDatabase != null)
            {
#if UNITY_EDITOR
                SerializedObject serializedObject = new SerializedObject(skinsDatabase);
                SerializedProperty tabs = serializedObject.FindProperty("tabs");

                serializedObject.Update();
                for(int i = 0; i < tabs.arraySize; i++)
                {
                    SerializedProperty tab = tabs.GetArrayElementAtIndex(i);

                    string name = tab.FindPropertyRelative("name").stringValue;
                    if (name == tabName)
                    {
                        tab.FindPropertyRelative("isActive").boolValue = state;

                        break;
                    }
                }

                serializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(skinsDatabase);
#endif
            }
            else
            {
                Debug.LogWarning("[Skin Store]: Skins database can't be found!");
            }
        }

        public static void SetTabState(SkinTab tabType, bool state)
        {
            SkinStoreDatabase skinsDatabase = RuntimeEditorUtils.GetAssetByName<SkinStoreDatabase>();
            if (skinsDatabase != null)
            {
#if UNITY_EDITOR
                SerializedObject serializedObject = new SerializedObject(skinsDatabase);
                SerializedProperty tabs = serializedObject.FindProperty("tabs");

                serializedObject.Update();
                for (int i = 0; i < tabs.arraySize; i++)
                {
                    SerializedProperty tab = tabs.GetArrayElementAtIndex(i);

                    SkinTab type = (SkinTab)tab.FindPropertyRelative("type").enumValueIndex;
                    if (type == tabType)
                    {
                        tab.FindPropertyRelative("isActive").boolValue = state;

                        break;
                    }
                }

                serializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(skinsDatabase);
#endif
            }
            else
            {
                Debug.LogWarning("[Skin Store]: Skins database can't be found!");
            }
        }
    }
}
