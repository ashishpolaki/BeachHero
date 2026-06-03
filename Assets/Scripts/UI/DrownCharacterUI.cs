using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class DrownCharacterUI : MonoBehaviour
    {
        [SerializeField] private Image timerImage;
        [SerializeField] private Canvas canvas;

        public void Awake()
        {
            canvas.worldCamera = Camera.main;
        }

        public void UpdateTimer(float waitTimePercentage)
        {
            timerImage.fillAmount = waitTimePercentage;
        }
    }
}
