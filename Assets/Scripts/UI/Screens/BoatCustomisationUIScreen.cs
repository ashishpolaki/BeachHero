using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatCustomisationUIScreen : BaseScreen
    {
        [SerializeField] private GameObject customisationPanel;
        [SerializeField] private BoatSkinUI boatSkinUIPrefab;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button customisationPanelCloseButton;
        [SerializeField] private Transform content;
        private bool isInitialized = false;
        private Dictionary<int, GameObject> boatSkinPrefabs = new Dictionary<int, GameObject>();

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            AddListeners();
            customisationPanel.SetActive(false);

            InitializeBoatCustomisation();
        }
        public override void Close()
        {
            base.Close();
            RemoveListeneres();
        }
        private void AddListeners()
        {
            homeButton.onClick.AddListener(OpenHome);
            customisationPanelCloseButton.onClick.AddListener(OnCustomisationPanelClose);
        }

        private void RemoveListeneres()
        {
            homeButton.onClick.RemoveAllListeners();
            customisationPanelCloseButton.onClick.RemoveAllListeners();
        }

        private void OnCustomisationPanelClose()
        {
            customisationPanel.SetActive(false);
        }

        private void OpenHome()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
        }

        public void OpenCustomisationPanel(int index)
        {
            customisationPanel.SetActive(true);
            var boatSkin = GameController.GetInstance.SkinController.BoatSkinsDatabase.BoatSkins[index];

            if (boatSkinPrefabs.TryGetValue(index, out var boatObject))
            {
                boatObject.SetActive(true);
            }
            else
            {
                var boatSkinPRefab = Instantiate(boatSkin.Prefab,this.transform);
                boatSkinPrefabs.Add(index, boatSkinPRefab.gameObject);
            }
        }

        private void InitializeBoatCustomisation()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;
            int boatIndex = SaveController.LoadInt(StringUtils.CURRENT_BOAT_INDEX, 0);
            foreach (var skinData in GameController.GetInstance.SkinController.BoatSkinsDatabase.BoatSkins)
            {
                bool isSelected = skinData.Index == boatIndex;
                var boatSkinUI = Instantiate(boatSkinUIPrefab, content);
                boatSkinUI.SetSkin(this,skinData, isSelected);
            }
        }
    }
}
