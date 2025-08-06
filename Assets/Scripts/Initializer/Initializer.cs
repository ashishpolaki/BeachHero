using DG.Tweening;
using UnityEngine;

namespace BeachHero
{
    public class Initializer : MonoBehaviour
    {
        private void Start()
        {
            Initialize();
        }
        private async void Initialize()
        {
            Application.targetFrameRate = 30;
            GameController.GetInstance.Init();
            AudioController.GetInstance.Init();
            AdController.GetInstance.Init();
            HapticsManager.GetInstance.Init();
            DOTween.Init();
            await UIController.GetInstance.LoadingUI.LoadSceneAsync(StringUtils.GAME_SCENE);
            GameController.GetInstance.SpawnLevel();
            await SceneLoader.GetInstance.UnloadScene(StringUtils.INIT_SCENE);
        }
    }
}
