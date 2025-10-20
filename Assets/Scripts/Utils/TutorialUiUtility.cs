using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    /// <summary>
    /// Utility helpers for managing tutorial UI overlays on GameObjects.
    /// </summary>
    public static class TutorialUiUtility
    {
        /// <summary>
        /// Ensures the target GameObject has a Canvas configured for tutorial overlay rendering
        /// and a GraphicRaycaster for input handling. Returns the Canvas component.
        /// </summary>
        /// <param name="target">GameObject to attach or validate UI components on.</param>
        /// <param name="sortingLayer">Optional sorting layer name. Defaults to "UI".</param>
        /// <param name="sortingOrder">Optional sorting order. Defaults to 10.</param>
        /// <returns>The Canvas attached to the GameObject.</returns>
        public static Canvas EnsureTutorialCanvas(GameObject target, string sortingLayer = "UI", int sortingOrder = 10)
        {
            if (target == null)
            {
                DebugUtils.LogError("TutorialUiUtility: EnsureTutorialCanvas called with null target.");
            }

            // Get or add a Canvas
            var canvas = target.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = target.AddComponent<Canvas>();
            }

            // Configure sorting settings
            if (!string.IsNullOrEmpty(sortingLayer))
            {
                canvas.overrideSorting = true;
                canvas.sortingLayerName = sortingLayer;
                canvas.sortingOrder = sortingOrder;
            }

            // Ensure input handling
            if (!target.TryGetComponent(out GraphicRaycaster _))
            {
                target.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        /// <summary>
        /// Removes any Canvas and GraphicRaycaster components from the target GameObject.
        /// </summary>
        /// <param name="target">The GameObject to clean up UI components from.</param>
        public static void RemoveTutorialCanvas(GameObject target)
        {
            if (target == null)
            {
                DebugUtils.LogError("TutorialUiUtility: RemoveTutorialCanvas called with null target.");
            }

            if (target.TryGetComponent(out GraphicRaycaster raycaster))
                Object.Destroy(raycaster);

            if (target.TryGetComponent(out Canvas canvas))
                Object.Destroy(canvas);
        }
    }
}
