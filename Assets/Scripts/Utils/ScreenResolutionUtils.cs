using UnityEngine;

namespace BeachHero
{
    public class ScreenResolutionUtils
    {
        public static Vector2 ReferenceResolution = new Vector2(1080, 1920);

        /// <summary>
        /// Returns the orthographic size based on current screen aspect ratio.
        /// </summary>
        public static float GetOrthographicSize(float referenceOrthoSize)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float currentAspect = screenWidth / screenHeight;
            float referenceAspect = ReferenceResolution.x / ReferenceResolution.y;
            float aspectScale = referenceAspect / currentAspect;
            float adjustedOrthoSize = referenceOrthoSize * aspectScale;
            return adjustedOrthoSize;
        }

        /// <summary>
        /// Returns the scale for a target object based on the current screen resolution and a reference resolution.
        /// </summary>
        public static Vector2 GetObjectScale(Vector2 referenceScale, float referenceOrthoSize)
        {
            float currentOrthoSize = GetOrthographicSize(referenceOrthoSize);
            float orthoScaleFactor = currentOrthoSize / referenceOrthoSize;
            float scaleY = referenceScale.y * orthoScaleFactor;

            return new Vector2(referenceScale.x, scaleY);
        }
    }
}
