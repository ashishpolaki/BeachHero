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
            if (currentScene != null)
            {
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
}