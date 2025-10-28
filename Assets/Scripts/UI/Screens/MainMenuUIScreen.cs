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
        [SerializeField] private Button settingsButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private UIButtonAudio[] buttonAnimationDatas;
        [SerializeField] private TweenSequencer panelOpenAnimation;
        [SerializeField] private Sprite playButtonSprite;

        private bool isWelcomeMessageShown = false;

        public override void Open(ScreenTabType screenTabType)
        {
            panelOpenAnimation.BuildSequence();
            base.Open(screenTabType);
            SetLevelNumber();
            AddListeners();
            panelOpenAnimation.Play();
        }

        public void OnPanelAnimationEnd()
        {
            isWelcomeMessageShown = SaveSystem.LoadBool("IsWelcomeMessageShown", false);
            if (!isWelcomeMessageShown)
            {
                SaveSystem.SaveBool("IsWelcomeMessageShown", true);
                isWelcomeMessageShown = true;
                Tween buttonTween = TutorialController.GetInstance.HighlightButton(levelPanelButton.transform, levelPanelButton.GetComponent<RectTransform>().sizeDelta, playButtonSprite, true);
                buttonTween.onComplete = () =>
                {
                    TutorialController.GetInstance.TutorialHand.ShowHandPointing(levelPanelButton.transform);
                    TutorialController.GetInstance.TutorialCharacter.PlayAnimation(TutorialCharacterType.WaveHand);
                };
            }
        }

        public override void Close()
        {
            base.Close();
            RemoveListeners();
            panelOpenAnimation.Kill();
        }

        private void AddListeners()
        {
            boatCustomisationButton.ButtonRegister(OnBoatCustomisationButtonClicked);
            levelPanelButton.ButtonRegister(OnPlayButtonClicked);
            storeButton.ButtonRegister(OnStoreButtonClicked);
            settingsButton.ButtonRegister(OnSettingsButtonClick);
        }

        private void RemoveListeners()
        {
            boatCustomisationButton.ButtonDeRegister(OnBoatCustomisationButtonClicked);
            levelPanelButton.ButtonDeRegister(OnPlayButtonClicked);
            storeButton.ButtonDeRegister(OnStoreButtonClicked);
            settingsButton.ButtonDeRegister(OnSettingsButtonClick);
        }
        private void OnSettingsButtonClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Settings, UIScreenEvent.Push);
        }

        private void OnBoatCustomisationButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.BoatCustomisation, UIScreenEvent.Open);
        }

        private void OnPlayButtonClicked()
        {
            if (isWelcomeMessageShown)
            {
                TutorialController.GetInstance.RemoveTutorialCanvas(levelPanelButton.gameObject);
                TutorialController.GetInstance.ClearButtonHighlight();
                TutorialController.GetInstance.TutorialHand.Hide();
                TutorialController.GetInstance.HideBlockerOverlay();
                TutorialController.GetInstance.TutorialCharacter.SkipAnimation();
            }

            MapController.GetInstance.CheckForMapUpdate();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
            GameController.GetInstance.SetGameState(GameState.Map);
            MapController.GetInstance.PlaceBoatAtCurrentLevel();
        }
        private void OnStoreButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Open);
        }

        private void SetLevelNumber()
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            levelNumberText.text = $"{currentLevelNumber}";
        }
    }
}
