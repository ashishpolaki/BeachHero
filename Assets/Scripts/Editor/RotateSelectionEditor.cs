#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomEditor(typeof(EditorSceneController))]
    public class RotateSelectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Coin Scene Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Open the movable Scene view panel, then select the coins you want to arrange or rotate.",
                MessageType.Info);

            string buttonLabel = CoinSceneToolOverlay.IsOpen ? "Close Coin Tool" : "Open Coin Tool";
            if (GUILayout.Button(buttonLabel, GUILayout.Height(26f)))
            {
                CoinSceneToolOverlay.Toggle((EditorSceneController)target);
            }
        }
    }
}
#endif
