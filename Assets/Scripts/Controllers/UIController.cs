using UnityEngine;

namespace BeachHero
{
    public enum UIScreenEvent
    {
        Open,           // Close current screen and open a new one
        Close,          // Close the current screen
        Show,           // Show the screen without affecting others (e.g., reappear)
        Hide,           // Hide the screen without destroying it
        Push,           // Open a new screen while keeping the current one active (stack-based UI)
        ChangeTab   // Change the active tab within the current screen
    }
    public class UIController : SingleTon<UIController>
    {
        #region Inspector Variables
        [SerializeField] private Canvas canvas;
        [SerializeField] private UIScreenManager screenManager;

        [Header("Fade")]
        [SerializeField] private FadeUI fadeUI;

        [Header("Notch SafeArea")]
        [SerializeField] private NotchSafeArea notchSafeArea;

        [Header("Loading")]
        [SerializeField] private LoadingUI loadingUI;
        #endregion

        #region Properties
        public bool IsScreenTransitioning { get; private set; }
        public bool IsInputBlocked { get; private set; }
        public bool IsLoadingScreenActive => loadingUI.IsLoading;
        public NotchSafeArea NotchSafeArea => notchSafeArea;
        public LoadingUI LoadingUI => loadingUI;
        public FadeUI FadeUI => fadeUI;
        public Canvas Canvas => canvas;
        #endregion

        #region Public Methods
        public void Init()
        {
        }
        public void ScreenEvent(ScreenType screenType, UIScreenEvent uIScreenEvent, ScreenTabType screenTabType = ScreenTabType.None)
        {
            screenManager.ScreenEvent(screenType, uIScreenEvent, screenTabType);
        }
        public Vector3 StarsPanelWorldPosition()
        {
            return screenManager.GetScreen<GameplayUIScreen>(ScreenType.Gameplay).StarsPanelWorldPosition;
        }
        public void StartTransition()
        {
            IsScreenTransitioning = true;
            screenManager.EnableCanvasGroup(false);
        }
        public void EndTransition()
        {
            IsScreenTransitioning = false;
            screenManager.EnableCanvasGroup(true);
        }
        public void CloseAllScreens()
        {
            screenManager.CloseAll();
        }
        public void BlockInput(bool block)
        {
            IsInputBlocked = block;
            //  screenManager.EnableCanvasGroup(!block);
        }
        #endregion
    }
}