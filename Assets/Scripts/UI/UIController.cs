using UnityEngine;

namespace BeachHero
{
    public enum UIScreenEvent
    {
        Open,
        Close,
        Show,
        Hide
    }
    public class UIController : SingleTon<UIController>
    {
        #region Inspector Variables
        [SerializeField] private UIScreenManager screenManager;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            screenManager.Initialize();
        }
        #endregion

        #region Public Methods
        public void ScreenEvent(ScreenType screenType, UIScreenEvent uIScreenEvent, ScreenTabType screenTabType = ScreenTabType.None)
        {
            screenManager.ScreenEvent(screenType, uIScreenEvent, screenTabType);
        }
        #endregion
    }
}