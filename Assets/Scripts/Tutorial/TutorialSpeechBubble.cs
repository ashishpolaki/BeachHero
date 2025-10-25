using TMPro;
using UnityEngine;

namespace BeachHero
{
    public class TutorialSpeechBubble : MonoBehaviour
    {
        [SerializeField] private GameObject bubbleBackground;
        [SerializeField] private TextMeshProUGUI messageText;

        /// <summary>
        /// Displays the speech bubble with the given message.
        /// </summary>
        public void Show(string message)
        {
            if (bubbleBackground != null)
                bubbleBackground.SetActive(true);

            if (messageText != null)
                messageText.text = message;
        }

        /// <summary>
        /// Hides the speech bubble.
        /// </summary>
        public void Hide()
        {
            if (bubbleBackground != null)
                bubbleBackground.SetActive(false);
        }
    }
}
