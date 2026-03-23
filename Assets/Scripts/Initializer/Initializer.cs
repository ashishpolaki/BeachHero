using System.Threading.Tasks;
using UnityEngine;

namespace BeachHero
{
    public class Initializer : MonoBehaviour
    {
        private void Awake()
        {
            GameController.GetInstance.LeaderboardController.InitializeGPGS();
        }

        private void Start()
        {
            AsyncLazyInit();
        }

        private void SetAdaptiveFrameRate()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            int ram = SystemInfo.systemMemorySize; // in MB
            string gpu = SystemInfo.graphicsDeviceName.ToLower()
                    .Replace("(tm)", "")
                    .Replace(" ", "")
                    .Trim();
            int targetFPS = 30; // default

            // --- Primary check: RAM-based classification ---
            if (ram > 6000) // tolerance check
            {
                targetFPS = 60; // high-end default
            }
            else
            {
                targetFPS = 30; // mid/low-end default
            }

            // --- Apply ---
            Application.targetFrameRate = targetFPS;
            DebugUtils.Log($"[AdaptiveFPS] GPU: {gpu} | RAM: {ram}MB | Target FPS: {targetFPS}");
#endif
        }

        private async void AsyncLazyInit()
        {
            SetAdaptiveFrameRate();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // small delay so the first frame renders smoothly
            await Task.Yield();
            GameController.GetInstance.Init();
            AudioController.GetInstance.Init();
            CameraController.GetInstance.Init();
            TutorialController.GetInstance.Init();
            AdController.GetInstance.Init();
            HapticsManager.GetInstance.Init();
            ES3.Init();
            Febucci.UI.Core.TAnimBuilder.InitializeGlobalDatabase();

            // wait a little to avoid freezing all at once
            await Task.Delay(100);
            await UIController.GetInstance.LoadingUI.LoadSceneAsync(StringUtils.GAME_SCENE);
            GameController.GetInstance.SpawnLevel();
            await SceneLoader.GetInstance.UnloadScene(StringUtils.INIT_SCENE);
            await UIController.GetInstance.LoadingUI.DisableLoadingScreen();
        }
    }
}
