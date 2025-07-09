using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class GameWinScreen : BaseScreen
    {
        [SerializeField] private Button nextLevelButton;

        private void OnEnable()
        {
            nextLevelButton.onClick.AddListener(OnNextLevel);
        }
        private void OnDisable()
        {
            nextLevelButton.onClick.RemoveListener(OnNextLevel);
        }
        private async void OnNextLevel()
        {
            await UIController.GetInstance.FadeInASync();
            Close();
            GameController.GetInstance.NextLevel();
            await SceneLoader.GetInstance.LoadScene(StringUtils.MAP_SCENE, 0);
            GameController.GetInstance.CameraController.DisableCameras();
            await UIController.GetInstance.FadeOutASync();
            MapController.GetInstance.MoveBoatFromPrevToCurrentLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
        }
    }
}
