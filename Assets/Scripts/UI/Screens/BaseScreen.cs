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
        Settings,
        Purchase
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
    [RequireComponent(typeof(TweenAnimator))]
    public class BaseScreen : MonoBehaviour, IScreen
    {
        #region Inspector Variables
        [SerializeField] private RectTransform rect;
        [SerializeField] private RectTransform notchSafeArea;
        [SerializeField] private ScreenType screenType;
        [SerializeField] private ScreenTabType defaultOpenTab;
        [SerializeField] private List<BaseScreenTab> tabs;
        [SerializeField] private TweenAnimator openAnimator;
        [SerializeField] private UiScreenTextStyler uiScreenTextStyler;
        #endregion

        #region Private Variables
        private ScreenTabType currentOpenTab;
        #endregion

        #region Properties 
        public TweenAnimator OpenAnimator => openAnimator;
        public ScreenType ScreenType => screenType;
        public List<BaseScreenTab> Tabs { get => tabs; }
        public ScreenTabType DefaultOpenTab { get => defaultOpenTab; }
        public ScreenTabType CurrentOpenTab { get => currentOpenTab; }
        public bool IsScreenOpen { get => gameObject.activeSelf; }
        public bool IsAnyTabOpened { get => tabs.Exists(tab => tab.IsOpen); }
        #endregion

        #region IScreen Implementation
        public virtual void Open(ScreenTabType screenTabType)
        {
            UIController.GetInstance.StartTransition();
            if (notchSafeArea != null)
            {
                UIController.GetInstance.NotchSafeArea.RegisterRectTransform(notchSafeArea);
            }
            if (uiScreenTextStyler != null)
            {
                uiScreenTextStyler.ApplyStyle();
            }
            openAnimator.BuildSequence();
            openAnimator.OnComplete(() => UIController.GetInstance.EndTransition());
            gameObject.SetActive(true);
            OpenInitialTab(screenTabType);
            OnScreenOpened();
        }
        public virtual void Close()
        {
            OpenAnimator.Kill();
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
        public virtual void OnScreenOpened()
        {
            //override in child classes if you want to do something when the screen is opened.
            openAnimator.Play();
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
    }
}
