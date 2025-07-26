using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatCustomisationUIScreen : BaseScreen
    {
        #region Inspector Variables
        [SerializeField] private BoatPurchasePanel purchasePanel;
        [SerializeField] private BoatSkinUI boatSkinPrefab;
        [SerializeField] private BoatSkinColorUI boatSkinColorUIPrefab;
        [SerializeField] private Transform boatsParent;
        [SerializeField] private Transform boatColorsParent;
        [SerializeField] private RectTransform screenRect;
        [SerializeField] private RectTransform boatsScrollRect;

        [SerializeField] private Button homeButton;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button nextBoatColorButton;
        [SerializeField] private Button prevBoatColorButton;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private Image currentBoatImg;
        #endregion

        #region Private Variables
        private int currentBoatIndex = -1;
        private int previousBoatIndex = -1;
        private int currentBoatColorIndex = 0;
        private bool isInitialized = false;
        private Dictionary<int, BoatSkinUI> boatSkins = new Dictionary<int, BoatSkinUI>();
        private List<BoatSkinColorUI> boatColorsList = new List<BoatSkinColorUI>();
        #endregion

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            currentBoatIndex = -1;
            previousBoatIndex = -1;
            AddListeners();
            InitializeBoatCustomisation();
            float actualScreenHeight = screenRect.rect.height;
            float boatsScrollPosY = boatsScrollRect.anchoredPosition.y;
            float adjustedHeight = actualScreenHeight + boatsScrollPosY;
            boatsScrollRect.sizeDelta = new Vector2(boatsScrollRect.sizeDelta.x, adjustedHeight);
        }
        public override void Close()
        {
            base.Close();
            boatSkins[currentBoatIndex].SetUnSelected();
            RemoveListeneres();
        }
        private void AddListeners()
        {
            homeButton.ButtonRegister(OpenHome);
            buyButton.ButtonRegister(OnBuyButtonClick);
            nextBoatColorButton.ButtonRegister(() => OnBoatColorChange(1));
            prevBoatColorButton.ButtonRegister(() => OnBoatColorChange(-1));
            purchasePanel.AddListeners();
            GameController.GetInstance.SkinController.OnBoatSkinPurchased += BoatSkinPurchased;
            GameController.GetInstance.SkinController.OnPurchaseFail += BoatSkinPurchasedFail;
        }
        private void RemoveListeneres()
        {
            homeButton.ButtonDeRegister();
            buyButton.ButtonDeRegister();
            nextBoatColorButton.ButtonDeRegister();
            prevBoatColorButton.ButtonDeRegister();
            purchasePanel.RemoveListeners();
            GameController.GetInstance.SkinController.OnBoatSkinPurchased -= BoatSkinPurchased;
            GameController.GetInstance.SkinController.OnPurchaseFail -= BoatSkinPurchasedFail;
        }
        private void OnBoatColorChange(int index)
        {
            int boatColorsCount = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentBoatIndex).SkinColors.Length;
            currentBoatColorIndex = (currentBoatColorIndex + index) % boatColorsCount;
            SaveSystem.SaveInt(StringUtils.CURRENT_BOAT_COLOR_INDEX + currentBoatIndex, currentBoatColorIndex);
            SetBoatColor(currentBoatColorIndex);
        }
        private void OnBuyButtonClick()
        {
            if (currentBoatIndex != -1)
            {
                purchasePanel.Open(currentBoatIndex, currentBoatColorIndex, currentBoatImg.sprite);
            }
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

                //Initialize the Boat Skins
                foreach (var skinData in GameController.GetInstance.SkinController.BoatSkinsDatabase.BoatSkins)
                {
                    var boatSkinUI = Instantiate(boatSkinPrefab, boatsParent);
                    boatSkinUI.SetSkin(this, skinData);
                    boatSkins.Add(skinData.Index, boatSkinUI);
                }
                for (int i = 0; i < 5; i++)
                {
                    var boatColorUI = Instantiate(boatSkinColorUIPrefab, boatColorsParent);
                    boatColorUI.gameObject.SetActive(false);
                    boatColorsList.Add(boatColorUI);
                }
            }
            ChangeBoat();
        }
        private void SetBoatInViewPanel()
        {
            var boatSkinSO = GameController.GetInstance.SkinController.BoatSkinsDatabase.GetBoatSkin(currentBoatIndex);
            speedSlider.value = boatSkinSO.SpeedMeter;
            currentBoatImg.sprite = boatSkinSO.SkinColors[currentBoatColorIndex].sprite;
        }
        private BoatSkinColorUI GetBoatColorObjectFromList()
        {
            foreach (var boatSkinColorUI in boatColorsList)
            {
                if (!boatSkinColorUI.gameObject.activeSelf)
                {
                    return boatSkinColorUI;
                }
            }
            var boatSkinColorObj = Instantiate(boatSkinColorUIPrefab, boatColorsParent);
            boatColorsList.Add(boatSkinColorObj);
            return boatSkinColorObj;
        }
        private void SetCurrentBoat()
        {
            boatSkins[currentBoatIndex].SetSelected();
            SetBoatInViewPanel();
            //Set the previous boat skin to unselected
            if (previousBoatIndex != -1)
            {
                boatSkins[previousBoatIndex].SetUnSelected();
            }
            previousBoatIndex = currentBoatIndex;
        }
        public void SetBoatColor(int colorIndex)
        {
            currentBoatColorIndex = colorIndex;
            // Deactivate all existing color UIs
            foreach (var boatColorUI in boatColorsList)
            {
                boatColorUI.UnSelect();
            }
            currentBoatImg.sprite = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentBoatIndex).SkinColors[currentBoatColorIndex].sprite;
            boatColorsList[currentBoatColorIndex].Select();
        }
        private void SetCurrentBoatColors()
        {
            // Deactivate all existing color UIs
            foreach (var boatColorUI in boatColorsList)
            {
                boatColorUI.gameObject.SetActive(false);
            }

            // Activate and initialize the boat colors for the current boat
            var boatSkin = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentBoatIndex);
            for (int i = 0; i < boatSkin.SkinColors.Length; i++)
            {
                var skinColorData = boatSkin.SkinColors[i];
                var boatSkinColorUI = GetBoatColorObjectFromList();
                int index = i;
                boatSkinColorUI.InitSkinColor(this, skinColorData, index, currentBoatColorIndex == index);
                boatSkinColorUI.gameObject.SetActive(true);
            }
        }

        public void ChangeBoat()
        {
            currentBoatIndex = GameController.GetInstance.SkinController.GetCurrentSelectedBoatIndex();
            currentBoatColorIndex = GameController.GetInstance.SkinController.GetCurrentSelectedBoatColorIndex(currentBoatIndex);
            SetCurrentBoat();
            SetCurrentBoatColors();
        }

        #region Purchase
        private void BoatSkinPurchased(int index)
        {
            //  purchasePanel.SetActive(true);
            //   purchaseDescriptionText.text = StringUtils.PRODUCT_PURCHASED_SUCCESS;
            //  boatSkins[index].Purchased();
        }
        private void BoatSkinPurchasedFail()
        {
            //   purchasePanel.SetActive(true);
            //   purchaseDescriptionText.text = StringUtils.PRODUCT_PURCHASE_FAILED;
        }
        #endregion
    }
}
