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
        [SerializeField] private BoatSkinColorUI boatSkinColorUIPrefab;
        [SerializeField] private Transform boatColorListContainer;
        //  [SerializeField] private BoatPurchasePanel purchasePanel;
        //  [SerializeField] private BoatSkinUI boatSkinPrefab;
        //  [SerializeField] private Transform boatListContainer;
        //  private Dictionary<int, BoatSkinUI> boatSkinMap = new Dictionary<int, BoatSkinUI>();
        //  [SerializeField] private Image selectedBoatImage;

        [SerializeField] private RectTransform screenBounds;
        [SerializeField] private RectTransform boatScrollView;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button nextBoatButton;
        [SerializeField] private Button prevBoatButton;
        [SerializeField] private TextMeshProUGUI purchaseBtnText;
        [SerializeField] private TextMeshProUGUI boatNameText;
        [SerializeField] private Slider speedBar;

        [Header("Camera Settings")]
        [SerializeField] private Camera customisationCamera;
        [SerializeField] private Vector3 camPositionOffset = new Vector3(2.55f, 2.82f, 4.87f);
        [SerializeField] private Vector3 camRotationOffset = new Vector3(17.191f, 205.917f, 357.561f);
        #endregion

        #region Private Variables
        private BoatSelectionAction boatSelectionAction = BoatSelectionAction.SelectSkin;
        private int selectedBoatIndex = -1;
        private int selectedColorIndex = 0;
        private bool isSetupComplete = false;
        private List<BoatSkinColorUI> colorUIList = new List<BoatSkinColorUI>();
        #endregion

        #region Override Methods
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            selectedBoatIndex = -1;
            AddListeners();
            SetupCustomisation();
            OpenAnimator.Play();
        }
        public override void Close()
        {
            base.Close();
            RemoveListeneres();
            //    purchasePanel.Close();
        }
        #endregion

        #region Button & Event Listeners
        private void AddListeners()
        {
            homeButton.ButtonRegister(OnHomePressed);
            purchaseButton.ButtonRegister(OnPurchasePressed);
            nextBoatButton.ButtonRegister(() => ChangeBoat(1));
            prevBoatButton.ButtonRegister(() => ChangeBoat(-1));
            //   purchasePanel.AddListeners();
            GameController.GetInstance.SkinController.OnSkinPurchased += BoatSkinPurchased;
            GameController.GetInstance.SkinController.OnSkinColorPurchased += BoatSkinColorPurchased;
            GameController.GetInstance.StoreController.OnBoatPurchaseFail += BoatSkinPurchasedFail;
        }
        private void RemoveListeneres()
        {
            homeButton.ButtonDeRegister(OnHomePressed);
            purchaseButton.ButtonDeRegister(OnPurchasePressed);
            nextBoatButton.ButtonDeRegister(() => ChangeBoat(1));
            prevBoatButton.ButtonDeRegister(() => ChangeBoat(-1));
            //  purchasePanel.RemoveListeners();
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
                //   purchasePanel.InitPurchase(selectedBoatIndex, selectedColorIndex, boatSelectionAction);
            }
        }
        private void OnHomePressed()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
        }
        //private void ChangeBoatColor(int direction)
        //{
        //    int colorCount = GameController.GetInstance.SkinController.GetBoatSkinByIndex(selectedBoatIndex).SkinColors.Length;
        //    selectedColorIndex = (selectedColorIndex + direction) % colorCount;
        //    ApplyBoatColor(selectedColorIndex);
        //}
        private void ChangeBoat(int direction)
        {
            int boatCount = GameController.GetInstance.SkinController.BoatSkinsDatabase.BoatSkins.Length;
            selectedBoatIndex = (selectedBoatIndex + direction + boatCount) % boatCount;
            UpdateSelectedBoat(selectedBoatIndex);
        }
        #endregion

        #region Setup/Init
        //private void AdjustBoatScrollViewHeight()
        //{
        //    float actualScreenHeight = screenBounds.rect.height;
        //    float boatsScrollPosY = boatScrollView.anchoredPosition.y;
        //    float adjustedHeight = actualScreenHeight + boatsScrollPosY;
        //    boatScrollView.sizeDelta = new Vector2(boatScrollView.sizeDelta.x, adjustedHeight);
        //}
        private void SetupCustomisation()
        {
            if (!isSetupComplete)
            {
                isSetupComplete = true;

                //Initialize the Boat Skins
                //foreach (var skinData in GameController.GetInstance.SkinController.BoatSkinsDatabase.BoatSkins)
                //{
                //    var boatSkinUI = Instantiate(boatSkinPrefab, boatListContainer);
                //    boatSkinUI.SetSkin(this, skinData);
                //    boatSkinMap.Add(skinData.Index, boatSkinUI);
                //}
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

        private void ApplyCameraTransform()
        {
            var target = GameController.GetInstance.LevelController.PlayerTransform;
            Vector3 rotatedOffset = target.rotation * camPositionOffset;
            customisationCamera.transform.position = target.position + rotatedOffset;
            customisationCamera.transform.rotation = target.rotation * Quaternion.Euler(camRotationOffset);
        }

        private void HighlightSelectedBoat()
        {
            GameController.GetInstance.LevelController.UpdateBoat(selectedBoatIndex, selectedColorIndex);
            ApplyCameraTransform();

            //Set Boat in Detail Panel
            var boatSkinSO = GameController.GetInstance.SkinController.GetBoatSkinByIndex(selectedBoatIndex);
            speedBar.value = boatSkinSO.SpeedMeter;
            boatNameText.text = boatSkinSO.Name;
        }

        private void UpdatePurchaseButton()
        {
            bool isBoatUnlocked = GameController.GetInstance.SkinController.IsBoatSkinUnlocked(selectedBoatIndex);

            if (!isBoatUnlocked)
            {
                boatSelectionAction = BoatSelectionAction.PurchaseSkin;
                purchaseBtnText.text = "BUY";
                purchaseButton.interactable = true;
                purchaseButton.gameObject.SetActive(true);
                bool isBoatColorUnlocked = GameController.GetInstance.SkinController.IsBoatSkinColorUnlocked(selectedBoatIndex, selectedColorIndex);
                if (isBoatColorUnlocked)
                {
                    purchaseButton.interactable = false;
                    purchaseButton.gameObject.SetActive(false);
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
                    purchaseButton.gameObject.SetActive(true);
                }
                else
                {
                    boatSelectionAction = BoatSelectionAction.SelectSkin;
                    if (selectedColorIndex == GameController.GetInstance.SkinController.GetSavedBoatColorIndex(selectedBoatIndex) &&
                        selectedBoatIndex == GameController.GetInstance.SkinController.GetSavedBoatIndex())
                    {
                        purchaseBtnText.text = "SELECT";
                        purchaseButton.interactable = false;
                        purchaseButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        // If the color is not selected, allow selection
                        purchaseBtnText.text = "SELECT";
                        purchaseButton.interactable = true;
                        purchaseButton.gameObject.SetActive(true);
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
            //  selectedBoatImage.sprite = GameController.GetInstance.SkinController.GetBoatSkinByIndex(selectedBoatIndex).SkinColors[selectedColorIndex].sprite;
            colorUIList[selectedColorIndex].Select();
            HighlightSelectedBoat();
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
            //  purchasePanel.Close();
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Push, ScreenTabType.PurchasSuccess);
            selectedBoatIndex = boatIndex;
            ApplyBoatColor(colorIndex);
        }
        private void BoatSkinPurchased(int index)
        {
            //  purchasePanel.Close();
            selectedBoatIndex = index;
            //  boatSkinMap[selectedBoatIndex].UpdateLockState();
            ApplyBoatColor(0); // Default to the first color after purchase
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Push, ScreenTabType.PurchasSuccess);
        }
        private void BoatSkinPurchasedFail()
        {
            //   purchasePanel.Close();
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Push, ScreenTabType.PurchasFail);
        }
        #endregion
    }
}
