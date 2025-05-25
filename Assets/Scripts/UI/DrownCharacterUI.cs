using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class DrownCharacterUI : MonoBehaviour
    {
        [SerializeField] private Image timerImage;

        public void UpdateTimer(float waitTimePercentage)
        {
            timerImage.fillAmount = waitTimePercentage;
        }
    }
}
