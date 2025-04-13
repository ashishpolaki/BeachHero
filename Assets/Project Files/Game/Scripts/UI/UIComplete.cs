using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;
using Watermelon.BeachRescue;

namespace Watermelon
{
    public class UIComplete : UIPage
    {
        private static readonly int COINS_HASH = "Coins".GetHashCode();

        private readonly string NO_THANKS_TEXT = "NO, THANKS";
        private readonly string CONTINUE_TEXT = "CONTINUE";

        [BoxGroup("References", "References")]
        [SerializeField] UIFadeAnimation backgroundFade;

        [BoxGroup("Top Panel", "Top Panel")]
        [SerializeField] CurrencyUIPanelSimple coinsPanelUI;

        [BoxGroup("Content", "Content")]
        [SerializeField] UIScaleAnimation levelCompleteLabel;

        [Space]
        [BoxGroup("Content")]
        [SerializeField] UIScaleAnimation rewardLabel;
        [BoxGroup("Content")]
        [SerializeField] TextMeshProUGUI rewardAmountText;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button multiplyRewardButton;
        [BoxGroup("Buttons")]
        [SerializeField] Button noThanksButton;
        [BoxGroup("Buttons")]
        [SerializeField] TMP_Text noThanksButtonText;

        private TweenCase noThanksAppearTween;

        private UIFadeAnimation multiplyRewardButtonFade;
        private UIFadeAnimation noThanksButtonFade;
        private UIScaleAnimation coinsPanelScalable;

        private int currentReward;

        public override void Init()
        {
            multiplyRewardButton.onClick.AddListener(MultiplyRewardButton);
            noThanksButton.onClick.AddListener(NoThanksButton);

            coinsPanelScalable = new UIScaleAnimation(coinsPanelUI);

            multiplyRewardButtonFade = new UIFadeAnimation(multiplyRewardButton);
            noThanksButtonFade = new UIFadeAnimation(noThanksButton);

            coinsPanelUI.Init();
        }

        #region Show/Hide
        public override void PlayShowAnimation()
        {
            rewardLabel.Hide(immediately: true);
            multiplyRewardButtonFade.Hide(immediately: true);
            multiplyRewardButton.interactable = false;
            noThanksButtonFade.Hide(immediately: true);
            noThanksButton.interactable = false;
            coinsPanelScalable.Hide(immediately: true);

            noThanksButtonText.text = NO_THANKS_TEXT;

            backgroundFade.Show(duration: 0.3f);
            levelCompleteLabel.Show();

            coinsPanelScalable.Show();

            currentReward = 25; // update reward here

            ShowRewardLabel(currentReward, false, 0.3f, delegate 
            {
                rewardLabel.Transform.DOPushScale(Vector3.one * 1.1f, Vector3.one, 0.2f, 0.2f).OnComplete(delegate
                {
                    FloatingCloud.SpawnCurrency(COINS_HASH, (RectTransform)rewardLabel.Transform, (RectTransform)coinsPanelScalable.Transform, 10, "", () =>
                    {
                        CurrenciesController.Add(CurrencyType.Coins, currentReward);

                        multiplyRewardButtonFade.Show();
                        multiplyRewardButton.interactable = true;

                        noThanksAppearTween = Tween.DelayedCall(1.5f, delegate
                        {
                            noThanksButtonFade.Show();
                            noThanksButton.interactable = true;
                        });
                    });
                });
            });
        }

        public override void PlayHideAnimation()
        {
            backgroundFade.Hide(0.25f);
            coinsPanelScalable.Hide();

            Tween.DelayedCall(0.25f, delegate
            {
                UIController.OnPageClosed(this);
            });
        }


        #endregion

        #region RewardLabel

        public void ShowRewardLabel(float rewardAmounts, bool immediately = false, float duration = 0.3f, Action onComplted = null)
        {
            rewardLabel.Show(immediately: immediately);

            if (immediately)
            {
                rewardAmountText.text = "+" + rewardAmounts;
                onComplted?.Invoke();

                return;
            }

            rewardAmountText.text = "+" + 0;

            Tween.DoFloat(0, rewardAmounts, duration, (float value) =>
            {

                rewardAmountText.text = "+" + (int)value;
            }).OnComplete(delegate
            {

                onComplted?.Invoke();
            });
        }

        #endregion

        #region Buttons

        public void MultiplyRewardButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            if (noThanksAppearTween != null && noThanksAppearTween.IsActive)
            {
                noThanksAppearTween.Kill();
            }

            noThanksButton.interactable = false;
            multiplyRewardButton.interactable = false;

            AdsManager.ShowRewardBasedVideo((bool success) =>
            {
                if (success)
                {
                    int rewardMult = 3;

                    noThanksButtonFade.Hide(immediately: true);
                    multiplyRewardButtonFade.Hide(immediately: true);

                    ShowRewardLabel(currentReward * rewardMult, false, 0.3f, delegate
                    {
                        FloatingCloud.SpawnCurrency(COINS_HASH, (RectTransform)rewardLabel.Transform, (RectTransform)coinsPanelScalable.Transform, 10, "", () =>
                        {
                            CurrenciesController.Add(CurrencyType.Coins, currentReward * rewardMult);

                            noThanksButtonText.text = CONTINUE_TEXT;

                            noThanksButton.interactable = true;
                            noThanksButton.gameObject.SetActive(true);
                            noThanksButtonFade.Show();
                        });
                    });
                }
                else
                {
                    NoThanksButton();
                }
            });
        }

        public void NoThanksButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            GameController.OnLevelCompletePanelClosed();
        }

        public void HomeButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            GameController.OnLevelCompletePanelClosed();
        }

        #endregion
    }
}
