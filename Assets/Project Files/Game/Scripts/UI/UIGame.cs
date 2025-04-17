using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Bokka.BeachRescue;

namespace Bokka
{
    public class UIGame : UIPage
    {
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform safeAreaRectTransform;

        [BoxGroup("Top Panel", "Top Panel")]
        [SerializeField] CurrencyUIPanelSimple coinsPanel;
        [BoxGroup("Top Panel")]
        [SerializeField] TextMeshProUGUI levelText;

        [BoxGroup("Other", "Other")]
        [SerializeField] Button skipButton;
        [BoxGroup("Other")]
        [SerializeField] Button undoButton;

        [Space]
        [BoxGroup("Other")]
        [SerializeField] GameObject tutorialHand;

        private CanvasGroup skipButtonGroup;
        private CanvasGroup undoButtonGroup;

        public override void Init()
        {
            coinsPanel.Init();

            skipButtonGroup = skipButton.GetComponent<CanvasGroup>();
            undoButtonGroup = undoButton.GetComponent<CanvasGroup>();

            skipButton.onClick.AddListener(OnSkipButtonClicked);
            undoButton.onClick.AddListener(OnUndoButtonClicked);

            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);
        }

        private void OnUndoButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            LevelController.Instance.Undo();
        }

        private void OnSkipButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            AdsManager.ShowRewardBasedVideo((reward) =>
            {
                if (reward)
                    GameController.Instance.SkipLevelFromGameplay();
            });
        }

        #region Show/Hide

        public override void PlayHideAnimation()
        {
            coinsPanel.Disable();

            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            levelText.text = $"LEVEL {GameController.CurrentLevelIndex + 1}";

            tutorialHand.SetActive(GameController.CurrentLevelIndex == 0);

            coinsPanel.Activate();

            UIController.OnPageOpened(this);
        }

        #endregion

        public void HideTutorial()
        {
            tutorialHand.SetActive(false);
        }
    }
}
