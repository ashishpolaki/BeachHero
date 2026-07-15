#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeachHero
{
    public static class BeachHeroSceneMenu
    {
        private const string SceneMenuRoot = "Beach Hero/Scenes/";

        private const string InitScenePath = "Assets/Scenes/Init.unity";
        private const string GameScenePath = "Assets/Scenes/Game.unity";
        private const string LevelEditorScenePath = "Assets/Scenes/GameEditorScene.unity";
        private const string MapEditorScenePath = "Assets/Scenes/MapEditorScene.unity";
        private const string TestScenePath = "Assets/Scenes/Test.unity";

        [MenuItem(SceneMenuRoot + "Init", false, 20)]
        private static void OpenInitScene()
        {
            OpenScene(InitScenePath);
        }

        [MenuItem(SceneMenuRoot + "Init", true)]
        private static bool ValidateInitScene()
        {
            return ValidateSceneMenu(SceneMenuRoot + "Init", InitScenePath);
        }

        [MenuItem(SceneMenuRoot + "Game", false, 21)]
        private static void OpenGameScene()
        {
            OpenScene(GameScenePath);
        }

        [MenuItem(SceneMenuRoot + "Game", true)]
        private static bool ValidateGameScene()
        {
            return ValidateSceneMenu(SceneMenuRoot + "Game", GameScenePath);
        }

        [MenuItem(SceneMenuRoot + "Level Editor", false, 22)]
        private static void OpenLevelEditorScene()
        {
            OpenScene(LevelEditorScenePath);
        }

        [MenuItem(SceneMenuRoot + "Level Editor", true)]
        private static bool ValidateLevelEditorScene()
        {
            return ValidateSceneMenu(SceneMenuRoot + "Level Editor", LevelEditorScenePath);
        }

        [MenuItem(SceneMenuRoot + "Map Editor", false, 23)]
        private static void OpenMapEditorScene()
        {
            OpenScene(MapEditorScenePath);
        }

        [MenuItem(SceneMenuRoot + "Map Editor", true)]
        private static bool ValidateMapEditorScene()
        {
            return ValidateSceneMenu(SceneMenuRoot + "Map Editor", MapEditorScenePath);
        }

        [MenuItem(SceneMenuRoot + "Test", false, 24)]
        private static void OpenTestScene()
        {
            OpenScene(TestScenePath);
        }

        [MenuItem(SceneMenuRoot + "Test", true)]
        private static bool ValidateTestScene()
        {
            return ValidateSceneMenu(SceneMenuRoot + "Test", TestScenePath);
        }

        private static bool ValidateSceneMenu(string menuPath, string scenePath)
        {
            Menu.SetChecked(menuPath, SceneManager.GetActiveScene().path == scenePath);
            return !EditorApplication.isPlayingOrWillChangePlaymode && AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null;
        }

        private static void OpenScene(string scenePath)
        {
            if (SceneManager.GetActiveScene().path == scenePath)
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(scenePath);
        }
    }
}
#endif
