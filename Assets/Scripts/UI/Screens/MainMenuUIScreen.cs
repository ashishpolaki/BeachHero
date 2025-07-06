using DG.Tweening;
using TMPro;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeachHero
{
    public class MainMenuUIScreen : BaseScreen
    {
        [SerializeField] private Button boatCustomisationButton;
        [SerializeField] private Button levelPanelButton;
        [SerializeField] private Button storeButton;
        [SerializeField] private Button mapButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private UIButtonAudio[] buttonAnimationDatas;

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
            mapButton.ButtonRegister(OnMapButtonClicked);
        }

        private void RemoveListeners()
        {
            boatCustomisationButton.ButtonDeRegister();
            levelPanelButton.ButtonDeRegister();
            storeButton.ButtonDeRegister();
            mapButton.ButtonDeRegister();
        }

        private void OnBoatCustomisationButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.BoatCustomisation, UIScreenEvent.Open);
        }

        private void OnLevelPanelButtonClicked()
        {
            OpenTab(ScreenTabType.PowerupSelection);
        }

        private void OnStoreButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Open);
        }

        private void OnMapButtonClicked()
        {
            var loading = SceneManager.LoadSceneAsync(StringUtils.MAP_SCENE, LoadSceneMode.Additive);
            loading.completed += (x) =>
                {
                    Scene loadedScene = SceneManager.GetSceneByName(StringUtils.MAP_SCENE);
                    SceneManager.SetActiveScene(loadedScene);
                };
        }
    }
}
