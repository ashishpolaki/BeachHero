using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MapUIScreen : BaseScreen
    {
        [SerializeField] private Button mapExitBtn;
        [SerializeField] private Button playButton;
        [SerializeField] private Button rightArrowBtn;
        [SerializeField] private Button leftArrowBtn;
        [SerializeField] private TextMeshProUGUI mapNameText;
        [SerializeField] private TextAnimatorPlayer unlockMapText;
        [SerializeField] private GameObject mapSelector;
        [SerializeField] private ParticleSystem confettiParticleSystem;

        [SerializeField] private float confettiDelay = 1f;
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

            var particle = confettiParticleSystem.main;
            particle.startDelay = confettiDelay;
            CameraController.GetInstance.SetActiveCamera(GameCameraType.Map);
            //  MapController.GetInstance.UpdatePathLine();
            MapController.GetInstance.SwitchMap(previousMapNumber, currentMapNumber);

            mapExitBtn.ButtonRegister(MapExitToHome);
            playButton.ButtonRegister(OnPlayButtonClick);
            rightArrowBtn.ButtonRegister(ScrollRight);
            leftArrowBtn.ButtonRegister(ScrollLeft);

            if (MapController.GetInstance != null)
            {
                MapController.GetInstance.OnMapButtonsEnabled += () => SetMapButtonsVisibility(true);
                MapController.GetInstance.OnShowPowerupSelection += PushPowerupSelectionScreen;
                MapController.GetInstance.OnMapUnlocked += NewMapUnlock;
            }
        }

        public override void Close()
        {
            base.Close();
            SetMapButtonsVisibility(false);
            ResetTextAnimator();
            MapController.GetInstance.SwitchMap(currentMapNumber, MapController.GetInstance.MapNumber);
            playButton.interactable = true;
            mapExitBtn.ButtonDeRegister(MapExitToHome);
            playButton.ButtonDeRegister(OnPlayButtonClick);
            rightArrowBtn.ButtonDeRegister(ScrollRight);
            leftArrowBtn.ButtonDeRegister(ScrollLeft);

            if (MapController.GetInstance != null)
            {
                MapController.GetInstance.OnMapButtonsEnabled -= () => SetMapButtonsVisibility(false);
                MapController.GetInstance.OnShowPowerupSelection -= PushPowerupSelectionScreen;
                MapController.GetInstance.OnMapUnlocked -= NewMapUnlock;
            }
        }

        private void NewMapUnlock()
        {
            isNewMapUnlocked = true;
            confettiParticleSystem.gameObject.SetActive(true);
            confettiParticleSystem.Play();
            unlockMapText.SetTypewriterSpeed(textApperanceSpeed);
            AudioController.GetInstance.PlaySound(AudioType.MapUnlock);
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
            }
        }

        private void SetMapButtonsVisibility(bool _val)
        {
            playButton.gameObject.SetActive(_val);
            mapExitBtn.gameObject.SetActive(_val);
        }

        private void PushPowerupSelectionScreen()
        {
            SetMapButtonsVisibility(true);
        }

        private void MapExitToHome()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            GameController.GetInstance.SetGameState(GameState.NotStarted);
        }

        private async void OnPlayButtonClick()
        {
            await UIController.GetInstance.FadeUI.FadeInASync();
            GameController.GetInstance.Play();
            GameController.GetInstance.LevelController.ResetAllSpawnedObjectsScale();
            await UIController.GetInstance.FadeUI.FadeOutASync();
            GameController.GetInstance.LevelController.PlaySpawnAnimations();
        }

        private void ScrollRight()
        {
            previousMapNumber = currentMapNumber;
            currentMapNumber += 1;
            UpdateMapVisual(true);
        }
        private void ScrollLeft()
        {
            previousMapNumber = currentMapNumber;
            currentMapNumber -= 1;
            UpdateMapVisual(true);
        }
        private void UpdateMapVisual(bool playAnim = false)
        {
            // bool isCurrentMap = MapController.GetInstance.MapNumber == currentMapNumber;
            //playButton.interactable = isCurrentMap;
            leftArrowBtn.interactable = currentMapNumber > 1;
            rightArrowBtn.interactable = currentMapNumber < totalMaps;
            mapNameText.text = $"{MapController.GetInstance.GetMapDescription(currentMapNumber)}";
            MapController.GetInstance.SwitchMap(previousMapNumber, currentMapNumber, playAnim);
        }
    }
}
