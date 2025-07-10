using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class GameLoseTab : BaseScreenTab
    {
        [SerializeField] private Button retryButton;
        [SerializeField] private Button skipLevelButton;

        public override void Open()
        {
            base.Open();
            retryButton.onClick.AddListener(OnRetryClick);
            skipLevelButton.onClick.AddListener(OnSkipLevelClick);
        }
        public override void Close()
        {
            base.Close();
            retryButton.onClick.RemoveListener(OnRetryClick);
            skipLevelButton.onClick.RemoveListener(OnSkipLevelClick);
        }
        private void OnSkipLevelClick()
        {
            AdController.GetInstance.ShowRewardedAd((reward) =>
            {
                // Callback after ad is watched.
                OnSkipLevelASync();
            });
        }
        private async void OnSkipLevelASync()
        {
            await UIController.GetInstance.FadeInASync();
            UIController.GetInstance.ScreenEvent(ScreenType.Results, UIScreenEvent.Close);
            GameController.GetInstance.SkipLevel();
            await SceneLoader.GetInstance.LoadScene(StringUtils.MAP_SCENE, 0);
            GameController.GetInstance.CameraController.DisableCameras();
            await UIController.GetInstance.FadeOutASync();
            MapController.GetInstance.MoveBoatFromPrevToCurrentLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
        }
        private void OnRetryClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.PowerupSelection, UIScreenEvent.Push);
            //   Close();
            //  GameController.GetInstance.RetryLevel();
        }
    }
}
