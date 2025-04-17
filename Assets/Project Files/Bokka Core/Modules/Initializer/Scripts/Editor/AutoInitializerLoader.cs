using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bokka
{
    [InitializeOnLoad]
    public static class AutoInitializerLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void LoadMain()
        {
            if (!CustomCoreSettings.AutoLoadInitializer) return;

            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name != "Init")
            {
                ("If You want to Play BeachHero Game, Please open Scene \"Init\" and Hit Play ").LogError();
                return;
            }
            if (currentScene.name != CustomCoreSettings.InitSceneName)
            {
                Initializer initializer = Object.FindObjectOfType<Initializer>();
                if (initializer == null)
                {
                    GameObject initializerPrefab = EditorUtils.GetAsset<GameObject>("Initializer");
                    if (initializerPrefab != null)
                    {
                        GameObject InitializerObject = Object.Instantiate(initializerPrefab);

                        initializer = InitializerObject.GetComponent<Initializer>();
                        initializer.Awake();
                        initializer.Init(false);
                    }
                    else
                    {
                        Debug.LogError("[Game]: Initializer prefab is missing!");
                    }
                }
            }
        }
    }
}