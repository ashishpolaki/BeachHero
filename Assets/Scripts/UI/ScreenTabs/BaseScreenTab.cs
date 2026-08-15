using UnityEngine;

namespace BeachHero
{
    public enum ScreenTabType
    {
        None,
        LevelTutorial,
        LevelWin,
        LevelFail,
        GamePause,
        PurchasSuccess,
        PurchasFail,
        InsufficientGameCurrency,
    }
    public interface IScreenTab
    {
        public ScreenTabType ScreenTabType { get; }
        public bool IsOpen { get; }
        public void Open();
        public void Close();
    }
    public class BaseScreenTab : MonoBehaviour, IScreenTab
    {
        [SerializeField] private RectTransform notchSafeArea;
        [SerializeField] private ScreenTabType screenTabType;

        public ScreenTabType ScreenTabType => screenTabType;
        public bool IsOpen { get => gameObject.activeSelf; }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }
        public virtual void Open()
        {
            if (notchSafeArea != null)
            {
                UIController.GetInstance.NotchSafeArea.RegisterRectTransform(notchSafeArea);
            }
            gameObject.SetActive(true);
        }
    }
}
