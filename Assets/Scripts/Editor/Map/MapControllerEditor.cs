#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    //[CustomEditor(typeof(MapController))]
    //public class MapControllerEditor : Editor
    //{
    //    public override void OnInspectorGUI()
    //    {
    //        // Draw the default inspector first
    //        DrawDefaultInspector();
    //        MapController mapController = (MapController)target;

    //        //if (GUILayout.Button("Generate Map 1 Level Visuals"))
    //        //{
    //        //    GenerateLevelVisuals(mapController);
    //        //}

    //        // Input field
    //        GUILayout.BeginHorizontal();
    //        int mapNumber = EditorGUILayout.IntField("Generate Levels", 1);
    //        if (GUILayout.Button("Generate"))
    //        {
    //            GenerateLevelVisuals(mapController, mapNumber);
    //        }
    //        GUILayout.EndHorizontal();
    //    }

    //}
}
#endif
