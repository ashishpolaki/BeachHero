using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Febucci.UI;

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
        [SerializeField] private TextAnimatorPlayer unlockMapText;
        [SerializeField] private TextAnimatorPlayer titleDescriptionText;
        [SerializeField] private GameObject mapSelector;

        [SerializeField] private float textApperanceSpeed = 3f;
        [SerializeField] private float textDisapperanceSpeed = 3f;
        [SerializeField] private float textDisappearDelay = 0.5f;

        private int currentMapNumber = 0;
        private int previousMapNumber = -1;
        private int totalMaps = 0;
        private bool isNewMapUnlocked = false;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            currentMapNumber = MapController.GetInstance.MapNumber;
            totalMaps = MapController.GetInstance.TotalMaps;
            zoomToggle.isOn = false;
            ZoomToggle(false);
            MapController.GetInstance.ChangeMapVisual(previousMapNumber, currentMapNumber);

            zoomToggle.onValueChanged.AddListener(ZoomToggle);
            mapExitBtn.ButtonRegister(MapExitToHome);
            playButton.ButtonRegister(OnPlayButtonClick);
            rightArrowBtn.ButtonRegister(() => ScrollRight());
            leftArrowBtn.ButtonRegister(() => ScrollLeft());

            if (MapController.GetInstance != null)
            {
                MapController.GetInstance.OnMapButtonsActive += () => SetMapButtonsVisibility(true);
                MapController.GetInstance.OnPushPowerupSelectionScreen += PushPowerupSelectionScreen;
                MapController.GetInstance.OnNewMapUnlockAction += NewMapUnlock;
            }
        }

        public override void Close()
        {
            base.Close();
            SetMapButtonsVisibility(false);
            ResetTextAnimator();

            zoomToggle.onValueChanged.RemoveListener(ZoomToggle);
            mapExitBtn.ButtonDeRegister();
            playButton.ButtonDeRegister();
            rightArrowBtn.ButtonDeRegister();
            leftArrowBtn.ButtonDeRegister();

            if (MapController.GetInstance != null)
            {
                MapController.GetInstance.OnMapButtonsActive -= () => SetMapButtonsVisibility(false);
                MapController.GetInstance.OnPushPowerupSelectionScreen -= PushPowerupSelectionScreen;
                MapController.GetInstance.OnNewMapUnlockAction -= NewMapUnlock;
            }
        }

        private void NewMapUnlock()
        {
            isNewMapUnlocked = true;
            unlockMapText.SetTypewriterSpeed(textApperanceSpeed);
            unlockMapText.ShowText($"{StringUtils.MAP_UNLOCKED_DESCRIPTION}<waitfor={textDisappearDelay}> ");
            unlockMapText.onTextShowed.AddListener(() =>
            {
                unlockMapText.SetTypewriterSpeed(textDisapperanceSpeed);
                unlockMapText.StartDisappearingText();
            });
        }

        private void ResetTextAnimator()
        {
            if (isNewMapUnlocked)
            {
                isNewMapUnlocked = false;
                unlockMapText.StopShowingText();
                unlockMapText.StopDisappearingText();
                unlockMapText.onTextShowed.RemoveAllListeners();
                titleDescriptionText.StopShowingText();
                titleDescriptionText.StopDisappearingText();
                titleDescriptionText.ShowText("");
            }
        }

        private void ShowTitleDescription()
        {
           titleDescriptionText.ShowText($"{MapController.GetInstance.GetMapDescription(currentMapNumber)}");
        }

        private void HideTitleDescription()
        {
            titleDescriptionText.StopShowingText();
            titleDescriptionText.StartDisappearingText();
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

        private void MapExitToHome()
        {
            //await UIController.GetInstance.FadeInASync();
            //await SceneLoader.GetInstance.UnloadScene(StringUtils.MAP_SCENE, IntUtils.MAP_SCENE_LOAD_DELAY);
            //  UIController.GetInstance.FadeOut();
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
                ShowTitleDescription();
            }
            else
            {
                HideTitleDescription();
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
            mapNameText.text = $"{MapController.GetInstance.GetMapName(currentMapNumber)}";
            bool isCurrentMap = MapController.GetInstance.MapNumber == currentMapNumber;
            playButton.interactable = isCurrentMap;
            zoomToggle.interactable = isCurrentMap;
            leftArrowBtn.interactable = currentMapNumber > 1;
            rightArrowBtn.interactable = currentMapNumber < totalMaps;
            ShowTitleDescription();
            MapController.GetInstance.ChangeMapVisual(previousMapNumber, currentMapNumber);
        }
    }
}
