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
        [SerializeField] private Transform titleFont;
        [SerializeField] private Transform tree;

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
            SetLevelNumber();
            AddListeners();
            OnAnimateBeachProps();
        }

        public override void Close()
        {
            base.Close();
            RemoveListeners();
            KillPropAnimations();
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
            GameController.GetInstance.SetGameState(GameState.Map);
            GameController.GetInstance.CameraController.DisableCameras();
            MapController.GetInstance.SetBoatInCurrentLevel();
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

        #region Beach Props
        private void OnAnimateBeachProps()
        {
            //Tree
            ShakeTree();

            //Title Font 
            titleFont.localScale = Vector3.zero;
            titleFont.DOScale(Vector3.one, 1).SetEase(Ease.OutFlash, 1f);
        }
        //462806//AE8653
        public void ShakeTree()
        {
            Sequence shakeLoop = DOTween.Sequence();
            shakeLoop.Append(tree.DOShakeRotation(0.3f, new Vector3(0, 0, 1), 10, 90, false, ShakeRandomnessMode.Harmonic));
            shakeLoop.AppendInterval(1); // Wait before the next loop starts
            shakeLoop.SetLoops(-1, LoopType.Restart); // Loop forever
        }

        private void KillPropAnimations()
        {
            if (tree != null)
            {
                tree.DOKill();
            }
            if (titleFont != null)
            {
                titleFont.DOKill();
            }
        }
        #endregion
    }
}
