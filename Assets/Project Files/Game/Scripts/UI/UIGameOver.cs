using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BeachRescue;

namespace Watermelon
{
    public class UIGameOver : UIPage
    {
        [BoxGroup("References", "References")]
        [SerializeField] UIFadeAnimation backgroundFade;

        [BoxGroup("Content", "Content")]
        [SerializeField] UIScaleAnimation levelFailed;

        private TweenCase closeTweenCase;

        public override void Init()
        {

        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            levelFailed.Hide(immediately: true);

            backgroundFade.Show(0.3f);

            levelFailed.Show(delay: 0.1f);

            closeTweenCase.KillActive();
            closeTweenCase = Tween.DelayedCall(1.5f, () => GameController.OnLevelFailedPanelClosed());

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            backgroundFade.Hide(0.3f);

            UIController.OnPageClosed(this);
        }

        #endregion
    }
}