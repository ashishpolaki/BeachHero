#if UNITY_EDITOR
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

namespace BeachHero
{
    public class TestLevel : MonoBehaviour
    {
        [SerializeField] private Material waterMaterial;

        private void Start()
        {
            ResetWaterMaterial();
            AsyncLazyInit();
        }

        /// <summary>
        /// Reset the water material properties to their default values.
        /// </summary>
        private void ResetWaterMaterial()
        {
            waterMaterial.SetFloat(Shader.PropertyToID($"{StringUtils.WHIRLPOOL_ENABLE}_{0}"), 0f);
            waterMaterial.SetFloat(Shader.PropertyToID($"{StringUtils.WHIRLPOOL_ENABLE}_{1}"), 0f);
            waterMaterial.SetFloat(Shader.PropertyToID($"{StringUtils.WHIRLPOOL_ENABLE}_{2}"), 0f);
        }

        private async void AsyncLazyInit()
        {
            DebugUtils.Log("Loading Game Scene");
            // small delay so the first frame renders smoothly
            await Task.Yield();
            GameController.GetInstance.Init();
            AudioController.GetInstance.Init();
            CameraController.GetInstance.Init();
            AdController.GetInstance.Init();
            HapticsManager.GetInstance.Init();
            DOTween.Init();
            Febucci.UI.Core.TAnimBuilder.InitializeGlobalDatabase();

            // wait a little to avoid freezing all at once
            await Task.Delay(100);
         //   await UIController.GetInstance.LoadingUI.LoadSceneAsync(StringUtils.GAME_SCENE);
            GameController.GetInstance.SpawnLevel();
            await UIController.GetInstance.LoadingUI.DisableLoadingScreen();
        }
    }
}
#endif
