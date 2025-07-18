using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public enum ScreenType
    {
        None,
        MainMenu,
        BoatCustomisation,
        Store,
        Gameplay,
        Results,
        Map,
        PowerupSelection,
        NoInternet,
        AdNotLoaded,
        Settings
    }
    public interface IScreen
    {
        public List<BaseScreenTab> Tabs { get; }
        public ScreenType ScreenType { get; }
        public ScreenTabType DefaultOpenTab { get; }
        public ScreenTabType CurrentOpenTab { get; }
        public bool IsScreenOpen { get; }
        public void OnScreenBack();
        public void Open(ScreenTabType screenTabType);
        public void Close();
        public void Show(ScreenTabType screenTabType);
        public void Hide();
        public void ChangeTab(ScreenTabType tab);
    }
    public enum UITweenAnimationType
    {
        None,
        ScalePunch,
        ScaleBounce,
        SlideInFromLeft,
        SlideInFromRight,
        SlideInFromTop,
        SlideInFromBottom,
        FlipIn
    }
    [System.Serializable]
    public struct TweenAnimationData
    {
        public UITweenAnimationType Type;
        public Ease Ease;
        public float Duration;
        public float Strength;
        public float Delay; // Delay before the animation starts
        public int Vibration;
        public float Offset;
        public float Elasticity;
    }
    public class BaseScreen : MonoBehaviour, IScreen
    {
        [SerializeField] private TweenAnimationData openingAnimationData;
        [SerializeField] private RectTransform rect;
        [SerializeField] private RectTransform notchSafeArea;
        [SerializeField] private ScreenType screenType;
        [SerializeField] private ScreenTabType defaultOpenTab;
        [SerializeField] private List<BaseScreenTab> tabs;

        private ScreenTabType currentOpenTab;

        public ScreenType ScreenType => screenType;
        public List<BaseScreenTab> Tabs { get => tabs; }
        public ScreenTabType DefaultOpenTab { get => defaultOpenTab; }
        public ScreenTabType CurrentOpenTab { get => currentOpenTab; }
        public bool IsScreenOpen { get => gameObject.activeSelf; }
        public bool IsAnyTabOpened { get => tabs.Exists(tab => tab.IsOpen); }

        #region IScreen Implementation
        public virtual void Open(ScreenTabType screenTabType)
        {
            if (notchSafeArea != null)
            {
                UIController.GetInstance.NotchSafeArea.RegisterRectTransform(notchSafeArea);
            }
            gameObject.SetActive(true);
            OpenInitialTab(screenTabType);
            PlayOpenAnimation(openingAnimationData);
        }
        public virtual void Close()
        {
            CloseAllTabs();
            gameObject.SetActive(false);
        }
        public virtual void Show(ScreenTabType screenTabType)
        {
            gameObject.SetActive(true);
            if (screenTabType != ScreenTabType.None)
            {
                OpenTab(screenTabType);
            }
        }
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        public virtual void OnScreenBack()
        {
            //Close the tab that is open and then return.
            if (currentOpenTab != ScreenTabType.None)
            {
                CloseTab(currentOpenTab);
            }
        }
        #endregion

        #region Tab Handling
        private void OpenInitialTab(ScreenTabType tab)
        {
            if (tab != ScreenTabType.None)
            {
                OpenTab(tab);
            }
            else if (defaultOpenTab != ScreenTabType.None)
            {
                OpenTab(defaultOpenTab);
            }
        }
        public void OpenTab(ScreenTabType screenTabType)
        {
            for (int i = 0; i < Tabs.Count; i++)
            {
                if (Tabs[i].ScreenTabType == screenTabType)
                {
                    currentOpenTab = screenTabType;
                    Tabs[i].Open();
                    break;
                }
            }
        }
        public void CloseTab(ScreenTabType screenTabType)
        {
            for (int i = 0; i < Tabs.Count; i++)
            {
                if (Tabs[i].ScreenTabType == screenTabType)
                {
                    Tabs[i].Close();
                    currentOpenTab = ScreenTabType.None;
                    break;
                }
            }
        }
        public void CloseAllTabs()
        {
            for (int i = 0; i < Tabs.Count; i++)
            {
                Tabs[i].Close();
            }
            currentOpenTab = ScreenTabType.None;
        }
        public void ChangeTab(ScreenTabType screenTabType)
        {
            if (currentOpenTab == screenTabType)
            {
                //If the tab is already open, do nothing.
                return;
            }
            //Close the current tab if it is open.
            if (currentOpenTab != ScreenTabType.None)
            {
                CloseTab(currentOpenTab);
            }
            //Open the new tab.
            OpenTab(screenTabType);
        }
        #endregion

        #region Animation
        private void PlayOpenAnimation(TweenAnimationData animationData)
        {
            switch (animationData.Type)
            {
                case UITweenAnimationType.ScalePunch:
                    rect.DOPunchScale(Vector3.one * animationData.Strength, animationData.Duration, animationData.Vibration, animationData.Elasticity).SetDelay(animationData.Delay);
                    break;
                case UITweenAnimationType.ScaleBounce:
                    rect.localScale = Vector3.zero;
                    rect.DOScale(Vector3.one, animationData.Duration).SetEase(animationData.Ease).SetDelay(animationData.Delay);
                    break;
                case UITweenAnimationType.SlideInFromLeft:
                    Vector2 originalPos = rect.anchoredPosition;
                    rect.anchoredPosition = new Vector2(-animationData.Offset, originalPos.y);
                    rect.DOAnchorPos(originalPos, animationData.Duration).SetEase(animationData.Ease).SetDelay(animationData.Delay);
                    break;
                case UITweenAnimationType.SlideInFromRight:
                    originalPos = rect.anchoredPosition;
                    rect.anchoredPosition = new Vector2(animationData.Offset, originalPos.y);
                    rect.DOAnchorPos(originalPos, animationData.Duration).SetEase(animationData.Ease);
                    break;
                case UITweenAnimationType.SlideInFromTop:
                    originalPos = rect.anchoredPosition;
                    rect.anchoredPosition = new Vector2(originalPos.x, animationData.Offset);
                    rect.DOAnchorPos(originalPos, animationData.Duration).SetEase(animationData.Ease);
                    break;
                case UITweenAnimationType.SlideInFromBottom:
                    originalPos = rect.anchoredPosition;
                    rect.anchoredPosition = new Vector2(originalPos.x, -animationData.Offset);
                    rect.DOAnchorPos(originalPos, animationData.Duration).SetEase(animationData.Ease);
                    break;
                case UITweenAnimationType.FlipIn:
                    //Rotates on Y-axis like a card flip
                    rect.localScale = Vector3.zero;
                    rect.localRotation = Quaternion.Euler(0, 180, 0); // Start with the back side facing up
                    rect.DOScale(Vector3.one, animationData.Duration).SetEase(Ease.OutBack);
                    rect.DORotate(new Vector3(0, 0, 0), animationData.Duration).SetEase(animationData.Ease);
                    break;
                default:
                    break;
            }
            PlayTweenAnimations(animationData);
        }

        protected virtual void PlayTweenAnimations(TweenAnimationData animationData)
        {
        }
        #endregion
    }
}
