using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MainMenuUIScreen : BaseScreen
    {
        [SerializeField] private Button playButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;
      
        private void OnEnable()
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
            levelNumberText.text = $"Level {(GameController.GetInstance.CurrentLevelIndex + 1).ToString()}";
        }

        private void OnDisable()
        {
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }
       
        private void OnPlayButtonClicked()
        {
            Close();
            GameController.GetInstance.Play();
        }
    }
}
