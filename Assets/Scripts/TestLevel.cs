#if UNITY_EDITOR
using DG.Tweening;
using UnityEngine;

namespace BeachHero
{
    public class TestLevel : MonoBehaviour
    {
        [SerializeField] private Material waterMaterial;

        private void Start()
        {
            ResetWaterMaterial();
            Initialize();
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

        private void Initialize()
        {
            Application.targetFrameRate = 30;
            GameController.GetInstance.Init();
            AudioController.GetInstance.Init();
            AdController.GetInstance.Init();
            HapticsManager.GetInstance.Init();
            DOTween.Init();
            GameController.GetInstance.SpawnLevel();
            GameController.GetInstance.Play();
        }
    }
}
#endif
