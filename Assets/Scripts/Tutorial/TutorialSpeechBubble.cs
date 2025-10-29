using Febucci.UI;
using TMPro;
using UnityEngine;

namespace BeachHero
{
    public class TutorialSpeechBubble : MonoBehaviour
    {
        [SerializeField] private GameObject bubbleBackground;
        [SerializeField] private RectTransform bubbleRect;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextAnimatorPlayer messageAnimator;

        /// <summary>
        /// Displays the speech bubble with the given message.
        /// </summary>
        public void Show(string message, Vector3 pos)
        {
            if(bubbleRect != null)
                bubbleRect.anchoredPosition = pos;

            if (bubbleBackground != null)
            {
                bubbleBackground.SetActive(true);
            }

            if (messageText != null)
            {
                messageText.gameObject.SetActive(true);
                messageAnimator.ShowText(message);
                //messageText.text = message;
            }
        }

        /// <summary>
        /// Hides the speech bubble.
        /// </summary>
        public void Hide()
        {
            if (bubbleBackground != null)
                bubbleBackground.SetActive(false);

            if (messageText != null)
            {
                messageAnimator.StopShowingText();
                messageAnimator.StopDisappearingText();
                messageText.text = string.Empty;
            }
        }
    }
}
