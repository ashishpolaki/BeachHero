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
        GameLose,
        GameWin,
        Results,
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

        public virtual void Open(ScreenTabType screenTabType)
        {
            gameObject.SetActive(true);
            //If screenTabType is not None then open the tab
            if (screenTabType != ScreenTabType.None)
            {
                OpenTab(screenTabType);
            }
            else
            {
                //else if defaultOpenTab is not None then open the defaulttab
                if (defaultOpenTab != ScreenTabType.None)
                {
                    OpenTab(defaultOpenTab);
                }
            }
            PlayOpenAnimation(openingAnimationData);
        }
        public virtual void Close()
        {
            foreach (var tab in Tabs)
            {
                if (tab.IsOpen)
                {
                    tab.Close();
                }
            }
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
                    currentOpenTab = ScreenTabType.None;
                    Tabs[i].Close();
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

        public virtual void OnScreenBack()
        {
            //Close the tab that is open and then return.
            if (currentOpenTab != ScreenTabType.None)
            {
                CloseTab(currentOpenTab);
                return;
            }
        }
    }
}
