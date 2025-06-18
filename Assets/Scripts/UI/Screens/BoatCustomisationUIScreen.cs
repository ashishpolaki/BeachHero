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
            boatSkins[currentBoatIndex].SetUnSelected();
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

        private void SetupCurrentBoatColors(BoatSkinSO boatSkin)
        {
            // Deactivate all existing color UIs
            foreach (var ui in boatSkinColorUIList)
                ui.gameObject.SetActive(false);

            // Set up color UIs for current skin
            for (int i = 0; i < boatSkin.SkinColors.Length; i++)
            {
                var skinColorData = boatSkin.SkinColors[i];
                var boatSkinColorUI = GetBoatSkinColorUI();
                boatSkinColorUI.InitSkinColor(this, skinColorData, i);
                boatSkinColorUI.gameObject.SetActive(true);
            }
        }

        private BoatSkinColorUI GetBoatSkinColorUI()
        {
            foreach (var boatSkinColorUI in boatSkinColorUIList)
            {
                if (!boatSkinColorUI.gameObject.activeSelf)
                {
                    return boatSkinColorUI;
                }
            }
            var boatSkinColorObj = Instantiate(boatSkinColorUIPrefab, boatColorContent);
            boatSkinColorUIList.Add(boatSkinColorObj);
            return boatSkinColorObj;
        }

        private void InitializeBoatCustomisation()
        {
            if (!isInitialized)
            {
                isInitialized = true;

                foreach (var skinData in GameController.GetInstance.SkinController.BoatSkinsDatabase.BoatSkins)
                {
                    var boatSkinUI = Instantiate(boatSkinUIPrefab, content);
                    boatSkinUI.SetSkin(this, skinData);
                    boatSkins.Add(skinData.Index, boatSkinUI);
                }
            }

            int boatIndex = SaveController.LoadInt(StringUtils.CURRENT_BOAT_INDEX, 1);
            SetCurrentBoatSelection(boatIndex);
        }
        #region public Methods
        public void SetBoatColor(int colorIndex)
        {
            var boatSkin = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentBoatIndex);
            boatEditImage.sprite = boatSkin.SkinColors[colorIndex].sprite;
        }

        public void OpenCustomisationPanel(int index, int colorIndex)
        {
            customisationPanel.SetActive(true);
            var boatSkin = GameController.GetInstance.SkinController.GetBoatSkinByIndex(index);
            boatEditImage.sprite = boatSkin.SkinColors[colorIndex].sprite;
            SetupCurrentBoatColors(boatSkin);
        }

        public void SetCurrentBoatSelection(int boatIndex)
        {
            previousBoatIndex = currentBoatIndex;
            currentBoatIndex = boatIndex;
            if (previousBoatIndex != -1)
            {
                boatSkins[previousBoatIndex].SetUnSelected();
            }
            boatSkins[currentBoatIndex].SetSelected();
        }
        #endregion

    }
}
