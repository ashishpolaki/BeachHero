using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    [System.Serializable]
    public class NotchSafeArea
    {
        [SerializeField] RectTransform[] safePanels;

        [Space]
        [SerializeField] bool conformX = true;
        [SerializeField] bool conformY = true;

        private List<RectTransform> registeredTransforms = new List<RectTransform>();

        private Rect lastSafeArea = new Rect(0, 0, 0, 0);
        private Vector2Int lastScreenSize = new Vector2Int(0, 0);

        private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

        public void Init()
        {
            if (safePanels != null && safePanels.Length > 0)
            {
                registeredTransforms.AddRange(safePanels);
            }

            Refresh();
        }

        public void RegisterRectTransform(RectTransform rectTransform)
        {
            if (!registeredTransforms.Contains(rectTransform))
            {
                registeredTransforms.Add(rectTransform);
                Refresh(true);
            }
        }

        public void Refresh(bool forceRefresh = false)
        {
            Rect safeArea = Screen.safeArea;

            if (safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y || Screen.orientation != lastOrientation || forceRefresh)
            {
                lastScreenSize.x = Screen.width;
                lastScreenSize.y = Screen.height;
                lastOrientation = Screen.orientation;

                 ApplySafeArea(safeArea);
            }
        }
        private void ApplySafeArea(RectTransform rectTransform)
        {
            lastSafeArea = Screen.safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width == 0 || Screen.height == 0)
                return;

            Rect safeRect = Screen.safeArea;

            if (!conformX)
            {
                safeRect.x = 0;
                safeRect.width = Screen.width;
            }

            if (!conformY)
            {
                safeRect.y = 0;
                safeRect.height = Screen.height;
            }

            Vector2 anchorMin = safeRect.position;
            Vector2 anchorMax = safeRect.position + safeRect.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
        private void ApplySafeArea(Rect rect)
        {
            lastSafeArea = rect;

            // Ignore x-axis?
            if (!conformX)
            {
                rect.x = 0;
                rect.width = Screen.width;
            }

            // Ignore y-axis?
            if (!conformY)
            {
                rect.y = 0;
                rect.height = Screen.height;
            }

            // Check for invalid screen startup state on some Samsung devices (see below)
            if (Screen.width > 0 && Screen.height > 0)
            {
                // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
                Vector2 anchorMin = rect.position;
                Vector2 anchorMax = rect.position + rect.size;

                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                // Fix for some Samsung devices (e.g. Note 10+, A71, S20) where Refresh gets called twice and the first time returns NaN anchor coordinates
                if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0)
                {
                    for (int i = 0; i < registeredTransforms.Count; i++)
                    {
                        if (registeredTransforms[i] != null)
                        {
                            registeredTransforms[i].anchorMin = anchorMin;
                            registeredTransforms[i].anchorMax = anchorMax;
                        }
                        else
                        {
                            registeredTransforms.RemoveAt(i);

                            i--;
                        }
                    }
                }
            }
        }
    }
}
