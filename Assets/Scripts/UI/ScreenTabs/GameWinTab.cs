using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class GameWinTab : BaseScreenTab
    {
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button _2xStarFishesButton;
   
        private async void OnNextLevel()
        {
            await UIController.GetInstance.FadeInASync();
            UIController.GetInstance.ScreenEvent(ScreenType.Results, UIScreenEvent.Close);
            GameController.GetInstance.NextLevel();
            await SceneLoader.GetInstance.LoadScene(StringUtils.MAP_SCENE, 0);
            GameController.GetInstance.CameraController.DisableCameras();
            await UIController.GetInstance.FadeOutASync();
            MapController.GetInstance.MoveBoatFromPrevToCurrentLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
        }
        public override void Open()
        {
            base.Open();
            nextLevelButton.onClick.AddListener(OnNextLevel);
            _2xStarFishesButton.onClick.AddListener(OnWatchAd);
        }
        public override void Close()
        {
            base.Close();
            nextLevelButton.onClick.RemoveListener(OnNextLevel);
            _2xStarFishesButton.onClick.RemoveListener(OnWatchAd);
        }
        private void OnWatchAd()
        {
            // Get 2x Star Fishes.
            AdController.GetInstance.ShowRewardedAd(null);
        }
    }
}
