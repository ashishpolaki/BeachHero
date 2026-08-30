using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    [System.Serializable]
    public class UIScreenManager
    {
        [SerializeField] private ScreenConfigSO screenConfig;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Canvas Holders")]
        [SerializeField] private Transform widthCanvasHolder;
        [SerializeField] private Transform heightCanvasHolder;
        [SerializeField] private Transform middleCanvasHolder;

        [Header("Canvas")]
        [SerializeField] private Canvas widthCanvas;
        [SerializeField] private Canvas heightCanvas;
        [SerializeField] private Canvas middleCanvas;

        private Dictionary<ScreenType, BaseScreen> screenCache = new Dictionary<ScreenType, BaseScreen>();
        private Stack<BaseScreen> screenStack = new Stack<BaseScreen>();
        private List<Canvas> canvasOrderList = new List<Canvas>();

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
                        Transform parentHolder = GetCanvasHolder(config.ScreenCanvasType);
                        var instance = GameObject.Instantiate(config, parentHolder);
                        screenCache[screenType] = instance;
                        SetCanvasOrder(instance.ScreenCanvasType);
                        return instance;
                    }
                }

                DebugUtils.LogError($"Screen not found for type: {screenType}");
                return null;
            }

            // Ensure the canvas order is initialized
            SetCanvasOrder(screenCache[screenType].ScreenCanvasType);

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
        private void SetCanvasOrder(ScreenCanvasType screenCanvasType)
        {
            var canvas = GetCanvas(screenCanvasType);
            var otherCanvas = GetCanvas(screenCanvasType == ScreenCanvasType.Height ? ScreenCanvasType.Width : ScreenCanvasType.Height);
            if (canvas != null)
            {
                canvas.sortingOrder = 1;
                otherCanvas.sortingOrder = 0;
            }
        }
        public void InitializeCanvasOrderList()
        {
            if (canvasOrderList.Count == 0)
            {
                if (heightCanvas != null) canvasOrderList.Add(heightCanvas);
                if (middleCanvas != null) canvasOrderList.Add(middleCanvas);
                if (widthCanvas != null) canvasOrderList.Add(widthCanvas);
            }
        }
        public void EnableCanvasGroup(bool enable)
        {
            //canvasGroup.interactable = enable;
            canvasGroup.blocksRaycasts = enable;
        }
        private Transform GetCanvasHolder(ScreenCanvasType canvasType)
        {
            switch (canvasType)
            {
                case ScreenCanvasType.Height:
                    return heightCanvasHolder != null ? heightCanvasHolder : widthCanvasHolder;
                case ScreenCanvasType.Middle:
                    return middleCanvasHolder != null ? middleCanvasHolder : widthCanvasHolder;
                case ScreenCanvasType.Width:
                default:
                    return widthCanvasHolder != null ? widthCanvasHolder : middleCanvasHolder;
            }
        }
        private Canvas GetCanvas(ScreenCanvasType canvasType)
        {
            switch (canvasType)
            {
                case ScreenCanvasType.Height:
                    return heightCanvas != null ? heightCanvas : widthCanvas;
                case ScreenCanvasType.Middle:
                    return middleCanvas != null ? middleCanvas : widthCanvas;
                case ScreenCanvasType.Width:
                default:
                    return widthCanvas != null ? widthCanvas : middleCanvas;
            }
        }
        #endregion
    }
}
