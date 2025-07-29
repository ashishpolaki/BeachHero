using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public enum BoatSelectionAction
    {
        PurchaseSkin,
        PurchaseSkinColor,
        SelectSkin
    }
    public class BoatCustomisationUIScreen : BaseScreen
    {
        #region Inspector Variables
        [SerializeField] private BoatPurchasePanel purchasePanel;
        [SerializeField] private BoatSkinUI boatSkinPrefab;
        [SerializeField] private BoatSkinColorUI boatSkinColorUIPrefab;
        [SerializeField] private Transform boatListContainer;
        [SerializeField] private Transform boatColorListContainer;

        [SerializeField] private RectTransform screenBounds;
        [SerializeField] private RectTransform boatScrollView;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button nextColorButton;
        [SerializeField] private Button prevColorButton;
        [SerializeField] private TextMeshProUGUI purchaseBtnText;
        [SerializeField] private Slider speedBar;
        [SerializeField] private Image selectedBoatImage;
        #endregion

        #region Private Variables
        private BoatSelectionAction boatSelectionAction = BoatSelectionAction.SelectSkin;
        private int selectedBoatIndex = -1;
        private int lastBoatIndex = -1;
        private int selectedColorIndex = 0;
        private bool isSetupComplete = false;
        private Dictionary<int, BoatSkinUI> boatSkinMap = new Dictionary<int, BoatSkinUI>();
        private List<BoatSkinColorUI> colorUIList = new List<BoatSkinColorUI>();
        #endregion

        #region Override Methods
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            selectedBoatIndex = -1;
            lastBoatIndex = -1;
            AddListeners();
            AdjustBoatScrollViewHeight();
            SetupCustomisation();
        }
        public override void Close()
        {
            base.Close();
            boatSkinMap[selectedBoatIndex].SetUnSelected();
            RemoveListeneres();
            purchasePanel.Close();
        }
        #endregion

        #region Button & Event Listeners
        private void AddListeners()
        {
            homeButton.ButtonRegister(OnHomePressed);
            purchaseButton.ButtonRegister(OnPurchasePressed);
            nextColorButton.ButtonRegister(() => ChangeBoatColor(1));
            prevColorButton.ButtonRegister(() => ChangeBoatColor(-1));
            purchasePanel.AddListeners();
            GameController.GetInstance.SkinController.OnSkinPurchased += BoatSkinPurchased;
            GameController.GetInstance.SkinController.OnSkinColorPurchased += BoatSkinColorPurchased;
            GameController.GetInstance.StoreController.OnBoatPurchaseFail += BoatSkinPurchasedFail;
        }
        private void RemoveListeneres()
        {
            homeButton.ButtonDeRegister();
            purchaseButton.ButtonDeRegister();
            nextColorButton.ButtonDeRegister();
            prevColorButton.ButtonDeRegister();
            purchasePanel.RemoveListeners();
            GameController.GetInstance.SkinController.OnSkinPurchased -= BoatSkinPurchased;
            GameController.GetInstance.SkinController.OnSkinColorPurchased -= BoatSkinColorPurchased;
            GameController.GetInstance.StoreController.OnBoatPurchaseFail -= BoatSkinPurchasedFail;
        }
        private void OnPurchasePressed()
        {
            if (boatSelectionAction == BoatSelectionAction.SelectSkin)
            {
                GameController.GetInstance.SkinController.SetSavedBoatIndex(selectedBoatIndex, selectedColorIndex);
                purchaseBtnText.text = "SELECT";
                purchaseButton.interactable = false;
            }
            else
            {
                purchasePanel.InitPurchase(selectedBoatIndex, selectedColorIndex, boatSelectionAction);
            }
        }
        private void OnHomePressed()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
        }
        private void ChangeBoatColor(int direction)
        {
            int colorCount = GameController.GetInstance.SkinController.GetBoatSkinByIndex(selectedBoatIndex).SkinColors.Length;
            selectedColorIndex = (selectedColorIndex + direction) % colorCount;
            ApplyBoatColor(selectedColorIndex);
        }
        #endregion

        #region Setup/Init
        private void AdjustBoatScrollViewHeight()
        {
            float actualScreenHeight = screenBounds.rect.height;
            float boatsScrollPosY = boatScrollView.anchoredPosition.y;
            float adjustedHeight = actualScreenHeight + boatsScrollPosY;
            boatScrollView.sizeDelta = new Vector2(boatScrollView.sizeDelta.x, adjustedHeight);
        }
        private void SetupCustomisation()
        {
            if (!isSetupComplete)
            {
                isSetupComplete = true;

                //Initialize the Boat Skins
                foreach (var skinData in GameController.GetInstance.SkinController.BoatSkinsDatabase.BoatSkins)
                {
                    var boatSkinUI = Instantiate(boatSkinPrefab, boatListContainer);
                    boatSkinUI.SetSkin(this, skinData);
                    boatSkinMap.Add(skinData.Index, boatSkinUI);
                }
                for (int i = 0; i < 5; i++)
                {
                    var boatColorUI = Instantiate(boatSkinColorUIPrefab, boatColorListContainer);
                    boatColorUI.gameObject.SetActive(false);
                    colorUIList.Add(boatColorUI);
                }
            }

            //Show the Previous Selected Boat Skin
            int boatIndex = GameController.GetInstance.SkinController.GetSavedBoatIndex();
            UpdateSelectedBoat(boatIndex);
        }
        #endregion

        private void HighlightSelectedBoat()
        {
            boatSkinMap[selectedBoatIndex].SetSelected();

            //Set Boat in Detail Panel
            var boatSkinSO = GameController.GetInstance.SkinController.GetBoatSkinByIndex(selectedBoatIndex);
            speedBar.value = boatSkinSO.SpeedMeter;
            selectedBoatImage.sprite = boatSkinSO.SkinColors[selectedColorIndex].sprite;

            //Set the previous boat skin to unselected
            if (lastBoatIndex != -1)
            {
                boatSkinMap[lastBoatIndex].SetUnSelected();
            }
            lastBoatIndex = selectedBoatIndex;
        }

        private void UpdatePurchaseButton()
        {
            bool isBoatUnlocked = GameController.GetInstance.SkinController.IsBoatSkinUnlocked(selectedBoatIndex);

            if (!isBoatUnlocked)
            {
                boatSelectionAction = BoatSelectionAction.PurchaseSkin;
                purchaseBtnText.text = "BUY";
                purchaseButton.interactable = true;

                bool isBoatColorUnlocked = GameController.GetInstance.SkinController.IsBoatSkinColorUnlocked(selectedBoatIndex, selectedColorIndex);
                if (!isBoatColorUnlocked)
                {
                    purchaseButton.interactable = false;
                }
            }
            else
            {
                bool isBoatColorUnlocked = GameController.GetInstance.SkinController.IsBoatSkinColorUnlocked(selectedBoatIndex, selectedColorIndex);
                if (!isBoatColorUnlocked)
                {
                    boatSelectionAction = BoatSelectionAction.PurchaseSkinColor;
                    purchaseBtnText.text = "BUY";
                    purchaseButton.interactable = true;
                }
                else
                {
                    boatSelectionAction = BoatSelectionAction.SelectSkin;
                    if (selectedColorIndex == GameController.GetInstance.SkinController.GetSavedBoatColorIndex(selectedBoatIndex) &&
                        selectedBoatIndex == GameController.GetInstance.SkinController.GetSavedBoatIndex())
                    {
                        purchaseBtnText.text = "SELECT";
                        purchaseButton.interactable = false;
                    }
                    else
                    {
                        // If the color is not selected, allow selection
                        purchaseBtnText.text = "SELECT";
                        purchaseButton.interactable = true;
                    }
                }
            }
        }

        public void UpdateSelectedBoat(int index)
        {
            selectedBoatIndex = index;
            selectedColorIndex = GameController.GetInstance.SkinController.GetSavedBoatColorIndex(selectedBoatIndex);
            HighlightSelectedBoat();
            ShowAvailableColors();
            UpdatePurchaseButton();
        }

        #region Boat Colors
        private void ShowAvailableColors()
        {
            // Deactivate all existing color UIs
            foreach (var boatColorUI in colorUIList)
            {
                boatColorUI.gameObject.SetActive(false);
            }

            // Activate and initialize the boat colors for the current boat
            var boatSkin = GameController.GetInstance.SkinController.GetBoatSkinByIndex(selectedBoatIndex);
            for (int i = 0; i < boatSkin.SkinColors.Length; i++)
            {
                var skinColorData = boatSkin.SkinColors[i];
                var boatSkinColorUI = GetReusableColorUI();
                int index = i;
                boatSkinColorUI.InitSkinColor(this, skinColorData, index, selectedColorIndex == index);
                boatSkinColorUI.gameObject.SetActive(true);
            }
        }
        public void ApplyBoatColor(int colorIndex)
        {
            selectedColorIndex = colorIndex;
            // Deactivate all existing color UIs
            foreach (var boatColorUI in colorUIList)
            {
                boatColorUI.UnSelect();
            }
            selectedBoatImage.sprite = GameController.GetInstance.SkinController.GetBoatSkinByIndex(selectedBoatIndex).SkinColors[selectedColorIndex].sprite;
            colorUIList[selectedColorIndex].Select();
            UpdatePurchaseButton();
        }
        private BoatSkinColorUI GetReusableColorUI()
        {
            foreach (var boatSkinColorUI in colorUIList)
            {
                if (!boatSkinColorUI.gameObject.activeSelf)
                {
                    return boatSkinColorUI;
                }
            }
            var boatSkinColorObj = Instantiate(boatSkinColorUIPrefab, boatColorListContainer);
            colorUIList.Add(boatSkinColorObj);
            return boatSkinColorObj;
        }
        #endregion

        #region Purchase
        private void BoatSkinColorPurchased(int boatIndex, int colorIndex)
        {
            purchasePanel.Close();
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Push, ScreenTabType.PurchasSuccess);
            selectedBoatIndex = boatIndex;
            ApplyBoatColor(colorIndex);
        }
        private void BoatSkinPurchased(int index)
        {
            purchasePanel.Close();
            selectedBoatIndex = index;
            boatSkinMap[selectedBoatIndex].UpdateLockState();
            ApplyBoatColor(0); // Default to the first color after purchase
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Push,ScreenTabType.PurchasSuccess);
        }
        private void BoatSkinPurchasedFail()
        {
            purchasePanel.Close();
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Push, ScreenTabType.PurchasFail);
        }
        #endregion
    }
}
