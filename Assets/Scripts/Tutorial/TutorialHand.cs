using UnityEngine;
using LitMotion;
using UnityEngine.UI;

namespace BeachHero
{
    public class TutorialHand : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform handRect;
        [SerializeField] private Canvas handCanvas;
        [SerializeField] private Image handImage;

        private HandAnimation currentHandAnimation;

        #region Initialization and Cleanup Methods
        public void Init()
        {
            if (!handRect) return;

            KillActiveTween();
            handRect.localScale = Vector3.one;
            handRect.gameObject.SetActive(false);
        }
        public void PlayAnimation(HandAnimation handAnimation)
        {
            currentHandAnimation?.Kill();
            currentHandAnimation = handAnimation;
            handRect.gameObject.SetActive(true);
            currentHandAnimation.Setup(handRect, handImage);
            currentHandAnimation.Play();
        }
        public void Hide()
        {
            KillActiveTween();
            if (handRect) handRect.gameObject.SetActive(false);
        }
        private void KillActiveTween()
        {
            if (currentHandAnimation != null)
            {
                currentHandAnimation?.Kill();
                currentHandAnimation = null;
            }
            //Reset hand sorting layer
            SetHandSortingLayer(StringUtils.SPRITES_ABOVE_UI_LAYER, 3);
        }
        public void SetHandSortingLayer(string sortingLayer, int sortingOrder)
        {
            if (handCanvas != null)
            {
                handCanvas.sortingLayerName = sortingLayer;
                handCanvas.sortingOrder = sortingOrder;
            }
        }
        #endregion
    }
}
