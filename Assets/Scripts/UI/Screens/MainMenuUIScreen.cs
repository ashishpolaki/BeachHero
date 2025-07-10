using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MainMenuUIScreen : BaseScreen
    {
        [SerializeField] private Button boatCustomisationButton;
        [SerializeField] private Button levelPanelButton;
        [SerializeField] private Button storeButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private UIButtonAudio[] buttonAnimationDatas;

        #region Tween Animations
        protected override void PlayTweenAnimations(TweenAnimationData animationData)
        {
            //base.PlayTweenAnimations(animationData);
            float delay = animationData.Delay + animationData.Duration;
            DOVirtual.DelayedCall(delay, ButtonTweenAnimations);
            // ButtonTweenAnimations();
        }
        private void ButtonTweenAnimations()
        {
            foreach (var buttonAnimationData in buttonAnimationDatas)
            {
                buttonAnimationData.PlayTweenAnimation();
            }
        }
        #endregion

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            levelNumberText.text = $"{currentLevelNumber}";
            AddListeners();
        }

        public override void Close()
        {
            base.Close();
            RemoveListeners();
        }

        private void AddListeners()
        {
            boatCustomisationButton.ButtonRegister(OnBoatCustomisationButtonClicked);
            levelPanelButton.ButtonRegister(OnLevelPanelButtonClicked);
            storeButton.ButtonRegister(OnStoreButtonClicked);
        }

        private void RemoveListeners()
        {
            boatCustomisationButton.ButtonDeRegister();
            levelPanelButton.ButtonDeRegister();
            storeButton.ButtonDeRegister();
        }

        private void OnBoatCustomisationButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.BoatCustomisation, UIScreenEvent.Open);
        }

        private async void OnLevelPanelButtonClicked()
        {
            //  OpenTab(ScreenTabType.PowerupSelection);
            UIController.GetInstance.FadeIn();
            await SceneLoader.GetInstance.LoadScene(StringUtils.MAP_SCENE, IntUtils.MAP_SCENE_LOAD_DELAY);
            UIController.GetInstance.FadeOut();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
            GameController.GetInstance.CameraController.DisableCameras();
            MapController.GetInstance.SetBoatInCurrentLevel();
        }

        private void OnStoreButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Open);
        }
    }
}
