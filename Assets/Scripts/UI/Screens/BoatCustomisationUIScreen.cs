using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatCustomisationUIScreen : BaseScreen
    {
        #region Inspector Variables
        [SerializeField] private BoatColorCustomisationPanel colorCustomisationPanel;
        [SerializeField] private BoatSkinUI boatSkinUIPrefab;
        [SerializeField] private Button homeButton;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject purchasePanel;
        [SerializeField] private TextMeshProUGUI purchaseDescriptionText;
        #endregion

        #region Private Variables
        private int currentBoatIndex = -1;
        private int previousBoatIndex = -1;
        private bool isInitialized = false;
        private Dictionary<int, BoatSkinUI> boatSkins = new Dictionary<int, BoatSkinUI>();
        #endregion

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            currentBoatIndex = -1;
            previousBoatIndex = -1;
            AddListeners();
            colorCustomisationPanel.Close();
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
            colorCustomisationPanel.AddListeners();
            GameController.GetInstance.SkinController.OnBoatSkinPurchased += BoatSkinPurchased;
            GameController.GetInstance.SkinController.OnPurchaseFail += BoatSkinPurchasedFail;
        }
        private void RemoveListeneres()
        {
            homeButton.onClick.RemoveAllListeners();
            colorCustomisationPanel.RemoveListeners();
            GameController.GetInstance.SkinController.OnBoatSkinPurchased -= BoatSkinPurchased;
            GameController.GetInstance.SkinController.OnPurchaseFail -= BoatSkinPurchasedFail;
        }
        private void OpenHome()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
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
            int boatIndex = SaveSystem.LoadInt(StringUtils.CURRENT_BOAT_INDEX, 1);
            SetCurrentBoatSelection(boatIndex);
        }

        #region Purchase
        private void BoatSkinPurchased(int index)
        {
            purchasePanel.SetActive(true);
            purchaseDescriptionText.text = StringUtils.PRODUCT_PURCHASED_SUCCESS;
            boatSkins[index].Purchased();
        }
        private void BoatSkinPurchasedFail()
        {
            purchasePanel.SetActive(true);
            purchaseDescriptionText.text = StringUtils.PRODUCT_PURCHASE_FAILED;
        }
        #endregion

        #region Public Methods
        public void OpenCustomisationPanel(int index, int colorIndex)
        {
            colorCustomisationPanel.Open(index,colorIndex);
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
