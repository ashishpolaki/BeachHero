using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MapUIScreen : BaseScreen
    {
        [SerializeField] private Toggle zoomToggle;
        [SerializeField] private Button mapExitBtn;
        [SerializeField] private Button playButton;
        [SerializeField] private Button rightArrowBtn;
        [SerializeField] private Button leftArrowBtn;
        [SerializeField] private TextMeshProUGUI mapNameText;
        [SerializeField] private GameObject mapSelector;

        private int currentMapNumber = 0;
        private int previousMapNumber = -1;
        private int totalMaps = 0;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            currentMapNumber = MapController.GetInstance.MapNumber;
            totalMaps = MapController.GetInstance.TotalMaps;
            UpdateMapVisual();
            ZoomToggle(false);

            zoomToggle.onValueChanged.AddListener(ZoomToggle);
            mapExitBtn.ButtonRegister(MapExitToHome);
            playButton.ButtonRegister(OnPlayButtonClick);
            rightArrowBtn.ButtonRegister(() => ScrollRight());
            leftArrowBtn.ButtonRegister(() => ScrollLeft());

            if (MapController.GetInstance != null)
            {
                MapController.GetInstance.OnMapButtonsActive += () => SetMapButtonsVisibility(true);
                MapController.GetInstance.OnPushPowerupSelectionScreen += PushPowerupSelectionScreen;
            }
        }

        public override void Close()
        {
            base.Close();
            SetMapButtonsVisibility(false);

            zoomToggle.onValueChanged.RemoveListener(ZoomToggle);
            mapExitBtn.ButtonDeRegister();
            playButton.ButtonDeRegister();
            rightArrowBtn.ButtonDeRegister();
            leftArrowBtn.ButtonDeRegister();

            if (MapController.GetInstance != null)
            {
                MapController.GetInstance.OnMapButtonsActive -= () => SetMapButtonsVisibility(false);
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

        private void ZoomToggle(bool isZoomOut)
        {
            if (isZoomOut)
            {
                MapController.GetInstance.ZoomOut();
            }
            else
            {
                MapController.GetInstance.ZoomIn();
            }
            mapSelector.gameObject.SetActive(isZoomOut);
        }
        private void ScrollRight()
        {
            previousMapNumber = currentMapNumber;
            currentMapNumber += 1;
            UpdateMapVisual();
        }
        private void ScrollLeft()
        {
            previousMapNumber = currentMapNumber;
            currentMapNumber -= 1;
            UpdateMapVisual();
        }
        private void UpdateMapVisual()
        {
            MapController.GetInstance.ChangeMapVisual(previousMapNumber, currentMapNumber);
            mapNameText.text = "MAP " + (currentMapNumber);
            bool isCurrentMap = MapController.GetInstance.MapNumber == currentMapNumber;
            playButton.interactable = isCurrentMap;
            zoomToggle.interactable = isCurrentMap;
            leftArrowBtn.interactable = currentMapNumber > 1;
            rightArrowBtn.interactable = currentMapNumber < totalMaps;
        }
    }
}
