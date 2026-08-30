using System.Threading.Tasks;
using UnityEngine;

namespace BeachHero
{
    public class Initializer : MonoBehaviour
    {
        private int loginType = 0;
        private void Awake()
        {
            SaveSystem.Init();
            loginType = SaveSystem.LoadInt(StringUtils.AUTH_LOGIN_TYPE, 0);
            if (loginType == 0)
            {
                UIController.GetInstance.LoadingUI.EnableLoadingScreen(false);
                UIController.GetInstance.ScreenEvent(ScreenType.Login, UIScreenEvent.Open, ScreenTabType.None);
            }
            else
            {
                UIController.GetInstance.LoadingUI.EnableLoadingScreen(true);
            }
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
            if (ram > 4000) // tolerance check
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
            if (Application.isEditor)
            {
                Application.targetFrameRate = 60;
            }
        }

        private async void AsyncLazyInit()
        {
            SetAdaptiveFrameRate();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // small delay so the first frame renders smoothly
            await Task.Yield();
            GameController.GetInstance.Init();
            UIController.GetInstance.Init();
            AudioController.GetInstance.Init();
            CameraController.GetInstance.Init();
            TutorialController.GetInstance.Init();
            RemoteConfig.GetInstance.Init();
            HapticsManager.GetInstance.Init();
            ParticleController.GetInstance.Initialize();
            Febucci.UI.Core.TAnimBuilder.InitializeGlobalDatabase();

            // wait a little to avoid freezing all at once
            await Task.Delay(100);

            // Check login status 
            if (loginType == 1 || loginType == 2)
            {
                await UIController.GetInstance.LoadingUI.LoadSceneAsync(StringUtils.GAME_SCENE);
                if (loginType == 1)   // GPGS login
                {
                    // Authenticates automatically loads + smart-merges cloud save
                    if (NetworkController.IsInternetAvailable)
                    {
                        await PlayGamesController.GetInstance.AutoAuthenticateAsync();
                    }
                }
                GameController.GetInstance.SpawnLevel();
                await SceneLoader.GetInstance.UnloadScene(StringUtils.INIT_SCENE);
                AdController.GetInstance.Init();
                await UIController.GetInstance.LoadingUI.DisableLoadingScreen();
            }
        }
    }
}

