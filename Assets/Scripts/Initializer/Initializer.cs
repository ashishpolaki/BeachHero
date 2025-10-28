using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

namespace BeachHero
{
    public class Initializer : MonoBehaviour
    {
        [SerializeField] private Material waterMaterial;

        private void Start()
        {
            ResetWaterMaterial();
            AsyncLazyInit();
        }

        private void SetAdaptiveFrameRate()
        {
            int ram = SystemInfo.systemMemorySize; // in MB
            string gpu = SystemInfo.graphicsDeviceName.ToLower()
                    .Replace("(tm)", "")
                    .Replace(" ", "")
                    .Trim();

#if UNITY_ANDROID && !UNITY_EDITOR
            int targetFPS = 30; // default

            // --- Primary check: RAM-based classification ---
            if (ram >= 8000)
            {
                targetFPS = 60; // high-end default
            }
            else
            {
                targetFPS = 30; // mid/low-end default
            }

           // Secondary check (GPU)
    if (!string.IsNullOrEmpty(gpu))
    {
        if (gpu.Contains("adreno7") || gpu.Contains("adreno8") || gpu.Contains("mali-g7") || gpu.Contains("immortalis"))
            targetFPS = 60;
        else if (gpu.Contains("adreno6") || gpu.Contains("mali-g6") || gpu.Contains("mali-g5"))
            targetFPS = Mathf.Min(targetFPS, 30);
    }

            // --- Apply ---
            Application.targetFrameRate = targetFPS;

            Debug.Log($"[AdaptiveFPS] GPU: {gpu} | RAM: {ram}MB | Target FPS: {targetFPS}");
#endif

        }




        private async void AsyncLazyInit()
        {
            SetAdaptiveFrameRate();

            // small delay so the first frame renders smoothly
            await Task.Yield();
            GameController.GetInstance.Init();
            AudioController.GetInstance.Init();
            CameraController.GetInstance.Init();
            TutorialController.GetInstance.Init();
            AdController.GetInstance.Init();
            HapticsManager.GetInstance.Init();
            DOTween.Init();
            Febucci.UI.Core.TAnimBuilder.InitializeGlobalDatabase();

            // wait a little to avoid freezing all at once
            await Task.Delay(100);
            await UIController.GetInstance.LoadingUI.LoadSceneAsync(StringUtils.GAME_SCENE);
            GameController.GetInstance.SpawnLevel();
            await SceneLoader.GetInstance.UnloadScene(StringUtils.INIT_SCENE);
            await UIController.GetInstance.LoadingUI.DisableLoadingScreen();
        }

        /// <summary>
        /// Reset the water material properties to their default values.
        /// </summary>
        private void ResetWaterMaterial()
        {
            waterMaterial.SetFloat(Shader.PropertyToID($"{StringUtils.WHIRLPOOL_ENABLE}_{1}"), 0f);
            waterMaterial.SetFloat(Shader.PropertyToID($"{StringUtils.WHIRLPOOL_ENABLE}_{2}"), 0f);
            waterMaterial.SetFloat(Shader.PropertyToID($"{StringUtils.WHIRLPOOL_ENABLE}_{3}"), 0f);
        }
    }
}
