using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeachHero
{
#if UNITY_EDITOR

    [InitializeOnLoad]
    public class InitSceneAutoLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Run()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name != "Init")
            {
                ("If You want to Play BeachHero Game, Please open Scene \"Init\" and Hit Play ").LogError();
                return;
            }
            // if (currentScene.name == CoreSettings.InitSceneName)
            // {
            //     Initializer initializer = Object.FindObjectOfType<Initializer>();
            //     if (initializer == null)
            //     {
            //         GameObject initializerPrefab = EditorUtils.GetAsset<GameObject>("Initializer");
            //         if (initializerPrefab != null)
            //         {
            //             GameObject InitializerObject = Object.Instantiate(initializerPrefab);
            //
            //             initializer = InitializerObject.GetComponent<Initializer>();
            //             initializer.Awake();
            //             initializer.Init(false);
            //         }
            //         else
            //         {
            //             Debug.LogError("[Game]: Initializer prefab is missing!");
            //         }
            //     }
            // }
        }
    }
#endif
}