using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatCustomisationUIScreen : BaseScreen
    {
        [SerializeField] private GameObject customisationPanel;
        [SerializeField] private BoatSkinUI boatSkinUIPrefab;
        [SerializeField] private BoatSkinColorUI boatSkinColorUIPrefab;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button customisationPanelCloseButton;
        [SerializeField] private Image boatEditImage;
        [SerializeField] private Transform content;
        [SerializeField] private Transform boatColorContent;

        private int currentBoatIndex = -1;
        private int previousBoatIndex = -1;
        private bool isInitialized = false;
        private Dictionary<int, BoatSkinUI> boatSkins = new Dictionary<int, BoatSkinUI>();
        private List<BoatSkinColorUI> boatSkinColorUIList = new List<BoatSkinColorUI>();

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            currentBoatIndex = -1;
            previousBoatIndex = -1;
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

        public void OpenCustomisationPanel(int index, int skinIndex)
        {
            customisationPanel.SetActive(true);
            var boatSkin = GameController.GetInstance.SkinController.BoatSkinsDatabase.BoatSkins[index];
            boatEditImage.sprite = boatSkin.SkinColors[skinIndex].sprite;
            for (int i = 0; i < boatSkin.SkinColors.Length; i++)
            {
                var skinColorData = boatSkin.SkinColors[i];
                bool isSelected = i == skinIndex;
                var boatSkinColorUI = Instantiate(boatSkinColorUIPrefab, boatColorContent);
                //   boatSkinColorUI.sets(this, skinColorData, isSelected, index, i);
                boatSkinColorUIList.Add(boatSkinColorUI);
            }
        }

        private void InitializeBoatCustomisation()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;
            foreach (var skinData in GameController.GetInstance.SkinController.BoatSkinsDatabase.BoatSkins)
            {
                var boatSkinUI = Instantiate(boatSkinUIPrefab, content);
                boatSkinUI.SetSkin(this, skinData);
                boatSkins.Add(skinData.Index, boatSkinUI);
            }

            int boatIndex = SaveController.LoadInt(StringUtils.CURRENT_BOAT_INDEX, 1);
            SetCurrentBoatSelection(boatIndex);
        }

        public void SetCurrentBoatSelection(int boatIndex)
        {
            previousBoatIndex = currentBoatIndex;
            currentBoatIndex = boatIndex;
            if(previousBoatIndex != -1)
            {
                boatSkins[previousBoatIndex].SetUnSelected();
            }
            boatSkins[currentBoatIndex].SetSelected();
        }
    }
}
