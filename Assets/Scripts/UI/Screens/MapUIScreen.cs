using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MapUIScreen : BaseScreen
    {
        [SerializeField] private Toggle zoomToggle;
        [SerializeField] private Button mapExitBtn;
        [SerializeField] private Button playButton;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            zoomToggle.onValueChanged.AddListener(ZoomToggle);
            zoomToggle.isOn = false;
            ZoomToggle(false); // Ensure map is zoomed in on open
            mapExitBtn.ButtonRegister(MapExitToHome);
            playButton.ButtonRegister(OnPlayButtonClick);
            MapController.GetInstance.OnMapButtonsActive += () =>
            {
                SetMapButtonsVisibility(true);
            };
            MapController.GetInstance.OnPushPowerupSelectionScreen += PushPowerupSelectionScreen;
        }

        public override void Close()
        {
            base.Close();
            zoomToggle.onValueChanged.RemoveListener(ZoomToggle);
            mapExitBtn.ButtonDeRegister();
            playButton.ButtonDeRegister();
            SetMapButtonsVisibility(false);
            if (MapController.GetInstance != null)
            {
                MapController.GetInstance.OnMapButtonsActive -= () =>
                {
                    SetMapButtonsVisibility(false);
                };
                MapController.GetInstance.OnPushPowerupSelectionScreen -= PushPowerupSelectionScreen;
            }
        }

        private void SetMapButtonsVisibility(bool _val)
        {
            playButton.gameObject.SetActive(_val);
            zoomToggle.gameObject.SetActive(_val);
            mapExitBtn.gameObject.SetActive(_val);
        }

        private void PushPowerupSelectionScreen()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.PowerupSelection, UIScreenEvent.Push);
            SetMapButtonsVisibility(true);
        }

        private async void MapExitToHome()
        {
            await UIController.GetInstance.FadeInASync();
            GameController.GetInstance.CameraController.EnableCameras();
            await SceneLoader.GetInstance.UnloadScene(StringUtils.MAP_SCENE, IntUtils.MAP_SCENE_LOAD_DELAY);
            UIController.GetInstance.FadeOut();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            GameController.GetInstance.SetGameState(GameState.NotStarted);
        }

        private void OnPlayButtonClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.PowerupSelection, UIScreenEvent.Push);
        }

        private void ZoomToggle(bool value)
        {
            if (value)
            {
                MapController.GetInstance.ZoomOut();
            }
            else
            {
                MapController.GetInstance.ZoomIn();
            }
        }
    }
}
