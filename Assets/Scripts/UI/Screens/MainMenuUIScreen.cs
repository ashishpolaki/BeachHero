using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MainMenuUIScreen : BaseScreen
    {
        [SerializeField] private Button boatCustomisationButton;
        [SerializeField] private Button levelPanelButton;
        [SerializeField] private Button storeButton;
        [SerializeField] private Button mapButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            levelNumberText.text = $"{currentLevelNumber}";
            AddListeners();
        }
        public override void Close()
        {
            base.Close();
            RemoveListeners();
        }
        private void AddListeners()
        {
            boatCustomisationButton.onClick.AddListener(OnBoatCustomisationButtonClicked);
            levelPanelButton.onClick.AddListener(OnLevelPanelButtonClicked);
            storeButton.onClick.AddListener(OnStoreButtonClicked);
            mapButton.onClick.AddListener(OnMapButtonClicked);
        }
        private void RemoveListeners()
        {
            boatCustomisationButton.onClick.RemoveListener(OnBoatCustomisationButtonClicked);
            levelPanelButton.onClick.RemoveListener(OnLevelPanelButtonClicked);
            storeButton.onClick.RemoveListener(OnStoreButtonClicked);
            mapButton.onClick.RemoveListener(OnMapButtonClicked);
        }
        private void OnBoatCustomisationButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.BoatCustomisation, UIScreenEvent.Open);
        }
        private void OnLevelPanelButtonClicked()
        {
            OpenTab(ScreenTabType.PowerupSelection);
        }
        private void OnStoreButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Open);
        }
        private void OnMapButtonClicked()
        {
        }
    }
}
