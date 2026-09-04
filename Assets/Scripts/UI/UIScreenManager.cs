using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    [System.Serializable]
    public class UIScreenManager
    {
        [SerializeField] private ScreenConfigSO screenConfig;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Transform canvasHolder;

        private Dictionary<ScreenType, BaseScreen> screenCache = new Dictionary<ScreenType, BaseScreen>();
        private Stack<BaseScreen> screenStack = new Stack<BaseScreen>();

        #region Screen Methods
        public T GetScreen<T>(ScreenType screenType) where T : BaseScreen
        {
            System.Type type = typeof(T);

            if (screenCache.TryGetValue(screenType, out BaseScreen screen))
            {
                return screen as T;
            }

            DebugUtils.LogError($"Screen of type {type} not found.");
            return null;
        }

        public void ScreenEvent(ScreenType screenType, UIScreenEvent uIEvent, ScreenTabType tabType)
        {
            switch (uIEvent)
            {
                case UIScreenEvent.Open:
                    OpenExclusive(screenType, tabType);
                    break;
                case UIScreenEvent.Close:
                    Close(screenType);
                    break;
                case UIScreenEvent.Show:
                    Show(screenType, tabType);
                    break;
                case UIScreenEvent.Hide:
                    Hide(screenType);
                    break;
                case UIScreenEvent.Push:
                    Push(screenType, tabType);
                    break;
                case UIScreenEvent.ChangeTab:
                    ChangeTab(screenType, tabType);
                    break;
            }
        }

        private void ChangeTab(ScreenType screenType, ScreenTabType tabType)
        {
            if (screenCache.TryGetValue(screenType, out var screen) && screen.IsScreenOpen)
            {
                screen.ChangeTab(tabType);
            }
        }
        private BaseScreen GetOrCreateScreen(ScreenType screenType)
        {
            // Check if the screen is already cached
            if (!screenCache.ContainsKey(screenType))
            {
                foreach (var config in screenConfig.screens)
                {
                    if (config.ScreenType == screenType)
                    {
                        var instance = GameObject.Instantiate(config, canvasHolder);
                        screenCache[screenType] = instance;
                        return instance;
                    }
                }

                DebugUtils.LogError($"Screen not found for type: {screenType}");
                return null;
            }
            return screenCache[screenType];
        }
        #endregion

        #region Screen Open/Show
        private void Show(ScreenType screenType, ScreenTabType tabType)
        {
            var screen = GetOrCreateScreen(screenType);
            screen.Show(tabType);
        }
        private void OpenExclusive(ScreenType screenType, ScreenTabType tabType)
        {
            CloseAll();
            Open(screenType, tabType);
        }
        private void Open(ScreenType screenType, ScreenTabType tabType)
        {
            var screen = GetOrCreateScreen(screenType);
            screen.Open(tabType);
            screen.transform.SetAsLastSibling();
            screenStack.Push(screen);
        }
        private void Push(ScreenType screenType, ScreenTabType tabType)
        {
            var screen = GetOrCreateScreen(screenType);
            screen.Open(tabType);
            screen.transform.SetAsLastSibling();
            screenStack.Push(screen);
        }
        #endregion

        #region Screen Hide/Close
        private void Hide(ScreenType screenType)
        {
            if (screenCache.TryGetValue(screenType, out var screen))
            {
                screen.Hide();
            }
        }
        private void Close(ScreenType screenType)
        {
            if (screenCache.TryGetValue(screenType, out var screen))
            {
                screen.Close();
                screenStack.TryPop(out _); // Remove from stack if it's on top
            }
        }
        public void CloseAll()
        {
            while (screenStack.Count > 0)
            {
                var screen = screenStack.Pop();
                screen.Close();
            }
        }
        #endregion

        #region Canvas
        public void EnableCanvasGroup(bool enable)
        {
            //canvasGroup.interactable = enable;
            canvasGroup.blocksRaycasts = enable;
        }
        #endregion
    }
}
