using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    [System.Serializable]
    public class UIScreenManager
    {
        [SerializeField] private ScreenConfigSO screenConfig;
        [SerializeField] private Transform uiHolder;
        private Dictionary<ScreenType, BaseScreen> screenCache = new Dictionary<ScreenType, BaseScreen>();
        private Stack<BaseScreen> screenStack = new Stack<BaseScreen>();

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

        private void ChangeTab(ScreenType screenType, ScreenTabType tabType)
        {
            if (screenCache.TryGetValue(screenType, out var screen) && screen.IsScreenOpen)
            {
                screen.ChangeTab(tabType);
            }
        }
        private BaseScreen GetOrCreateScreen(ScreenType screenType)
        {
            if (!screenCache.ContainsKey(screenType))
            {
                foreach (var config in screenConfig.screens)
                {
                    if (config.ScreenType == screenType)
                    {
                        var instance = GameObject.Instantiate(config, uiHolder);
                        screenCache[screenType] = instance;
                        return instance;
                    }
                }

                DebugUtils.LogError($"Screen not found for type: {screenType}");
                return null;
            }

            return screenCache[screenType];
        }
        private void Show(ScreenType screenType, ScreenTabType tabType)
        {
            var screen = GetOrCreateScreen(screenType);
            screen.Show(tabType);
        }
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
    }
}
