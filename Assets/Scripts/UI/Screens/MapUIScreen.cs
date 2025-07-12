using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MapUIScreen : BaseScreen
    {
        [SerializeField] private Toggle zoomToggle;
        [SerializeField] private Button mapExitBtn;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            zoomToggle.onValueChanged.AddListener(ZoomToggle);
            zoomToggle.isOn = false;
            ZoomToggle(false); // Ensure map is zoomed in on open
            mapExitBtn.ButtonRegister(MapExit);
        }

        public override void Close()
        {
            base.Close();
            zoomToggle.onValueChanged.RemoveListener(ZoomToggle);
            mapExitBtn.ButtonDeRegister();
        }
        private async void MapExit()
        {
            await UIController.GetInstance.FadeInASync();
            GameController.GetInstance.CameraController.EnableCameras();
            await SceneLoader.GetInstance.UnloadScene(StringUtils.MAP_SCENE, IntUtils.MAP_SCENE_LOAD_DELAY);
            UIController.GetInstance.FadeOut();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
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
