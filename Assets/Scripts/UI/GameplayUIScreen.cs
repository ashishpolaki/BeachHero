using TMPro;
using UnityEngine;

namespace BeachHero
{
    public class GameplayUIScreen : BaseScreen
    {
        [SerializeField] private TextMeshProUGUI timerTxt;

        private void OnEnable()
        {
            GameController.GetInstance.LevelController.OnLevelTimerUpdate += UpdateTimer;
        }
        private void OnDisable()
        {
            if (GameController.Exists)
                GameController.GetInstance.LevelController.OnLevelTimerUpdate -= UpdateTimer;
        }
        private void UpdateTimer(float time)
        {
            timerTxt.text = time.ToString("00");
        }
    }
}
