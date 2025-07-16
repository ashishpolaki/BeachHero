using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeachHero
{
    public class SceneLoader : SingleTon<SceneLoader>
    {
        public async Task LoadScene(string name, int milliSeconds = 0, Action action = null)
        {
            var asyncOperation = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;

            while (asyncOperation.progress < 0.9f)
                await Task.Yield();

            await Task.Delay(milliSeconds);

            // Activate the scene after it's fully loaded
            asyncOperation.allowSceneActivation = true;

            while (!asyncOperation.isDone)
                await Task.Yield();

            Scene loadedScene = SceneManager.GetSceneByName(name);
            SceneManager.SetActiveScene(loadedScene);
        }

        public async Task UnloadScene(string name, int milliSeconds = 0)
        {
            await Task.Delay(milliSeconds);
          
            var asyncOperation = SceneManager.UnloadSceneAsync(name, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            while (!asyncOperation.isDone)
                await Task.Yield();
        }
        //IEnumerator UnloadMapSceneAsync()
        //{
        //    UIController.GetInstance.FadeIn();
        //    // Optional: wait for fade-in to complete
        //    yield return new WaitForSeconds(0.5f); // or wait for FadeIn to finish
        //    SceneManager.UnloadSceneAsync(StringUtils.MAP_SCENE, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        //    GameController.GetInstance.CameraController.EnableCameras();
        //    UIController.GetInstance.FadeOut();
        //    UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
        //}

        //IEnumerator LoadMapSceneAsync()
        //{

        //    UIController.GetInstance.FadeIn();

        //    var loading = SceneManager.LoadSceneAsync(StringUtils.MAP_SCENE, LoadSceneMode.Additive);
        //    loading.allowSceneActivation = false;
        //    // Wait until scene is almost ready
        //    while (loading.progress < 0.9f)
        //        yield return null;

        //    // Optional: wait for fade-in to complete
        //    yield return new WaitForSeconds(0.5f); // or wait for FadeIn to finish

        //    loading.allowSceneActivation = true;

        //    // Wait until loading is complete
        //    yield return new WaitUntil(() => loading.isDone);

        //    Scene loadedScene = SceneManager.GetSceneByName(StringUtils.MAP_SCENE);
        //    SceneManager.SetActiveScene(loadedScene);

        //    UIController.GetInstance.FadeOut();
        //    UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
        //    GameController.GetInstance.CameraController.DisableCameras();
        //    MapController.GetInstance.SetBoatInCurrentLevel();
        //}
    }
}
