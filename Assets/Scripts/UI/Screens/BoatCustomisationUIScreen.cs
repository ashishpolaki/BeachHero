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
        [Header("Color Swatches")]
        [SerializeField] private BoatSkinColorUI boatSkinColorUIPrefab;
        [SerializeField] private Transform boatColorListContainer;

        [Header("Preview Settings")]
        [SerializeField] private Transform previewBoatParent;
        [SerializeField] private Vector3 previewBoatPositionOffset = new Vector3(50f, 0f, 0f);
        [SerializeField] private Vector3 camPositionOffset = new Vector3(2.55f, 2.82f, 4.87f);
        [SerializeField] private Vector3 camRotationOffset = new Vector3(17.191f, 205.917f, 357.561f);

        [Header("Navigation Controls")]
        [SerializeField] private UIButton backButton;
        [SerializeField] private UIButton nextBoatButton;
        [SerializeField] private UIButton prevBoatButton;

        [Header("Boat Info & Status")]
        [SerializeField] private TextMeshProUGUI boatNameText;
        [SerializeField] private GameObject lockObject;

        [Header("Purchase Settings")]
        [SerializeField] private UIButton equipButton;
        [SerializeField] private UIButton purchaseButton;
        [SerializeField] private GameObject purchaseButtonContainer;
        [SerializeField] private TextMeshProUGUI purchaseBtnText;
        [SerializeField] private TextMeshProUGUI equipBtnText;
        [SerializeField] private TweenAnimator purchaseButtonAnimator;

        [Header("Speed Gauge Settings")]
        [SerializeField] private Image[] speedBarImages;
        [SerializeField] private Sprite speedBarFillSprite;
        [SerializeField] private Sprite speedBarUnfilledSprite;
        [SerializeField] private Transform speedNeedleTransform;
        [SerializeField] private float speedNeedleMinAngle = 100f;
        [SerializeField] private float speedNeedleMaxAngle = -100f;
        #endregion

        #region Private Variables
        private BoatSelectionAction currentAction = BoatSelectionAction.SelectSkin;
        private int selectedBoatIndex = -1;
        private int selectedColorIndex = 0;
        private List<BoatSkinColorUI> colorUIList = new List<BoatSkinColorUI>();
        private Dictionary<int, Boat> previewBoatsCache = new Dictionary<int, Boat>();
        private Boat currentPreviewBoat;
        #endregion

        #region Controller Properties
        private SkinController SkinController => GameController.GetInstance.SkinController;
        private StoreManager StoreController => GameController.GetInstance.StoreController;
        private LevelController LevelController => GameController.GetInstance.LevelController;
        #endregion

        #region Override Methods
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            SetBoatNotificationShown();
            AddListeners();
            SetupCustomisation();

            //purchase button animation setup
            if (purchaseButtonAnimator != null)
            {
                purchaseButtonAnimator.BuildSequence();
            }
        }

        private void SetBoatNotificationShown()
        {
            if (SaveSystem.CurrentData != null && !SaveSystem.CurrentData.isBoatNotificationShown)
            {
                SaveSystem.CurrentData.SetBoatNotificationShown(true);
                SaveSystem.SaveGameData();
            }
        }

        public override void Close()
        {
            base.Close();
            if (CameraController.GetInstance != null)
            {
                CameraController.GetInstance.SetPreviewCameraEnabled(PreviewCameraType.BoatCustomisation, false);
            }
            if (currentPreviewBoat != null)
            {
                currentPreviewBoat.gameObject.SetActive(false);
            }
            RemoveListeners();

            //purchase button animation cleanup
            if (purchaseButtonAnimator != null)
            {
                purchaseButtonAnimator.Kill();
            }
        }
        #endregion

        #region Button & Event Listeners
        private void AddListeners()
        {
            if (backButton != null) backButton.OnButtonReleased += OnBackOrHomePressed;
            if (purchaseButton != null) purchaseButton.OnButtonReleased += OnActionPressed;
            if (equipButton != null) equipButton.OnButtonReleased += OnActionPressed;
            if (nextBoatButton != null) nextBoatButton.OnButtonReleased += OnNextBoatPressed;
            if (prevBoatButton != null) prevBoatButton.OnButtonReleased += OnPrevBoatPressed;

            if (SkinController != null)
            {
                SkinController.BoatCustomisationScreenActive(true);
                SkinController.OnSkinPurchased += OnBoatSkinPurchased;
                SkinController.OnSkinColorPurchased += OnBoatSkinColorPurchased;
            }
        }

        private void RemoveListeners()
        {
            if (backButton != null) backButton.OnButtonReleased -= OnBackOrHomePressed;
            if (purchaseButton != null) purchaseButton.OnButtonReleased -= OnActionPressed;
            if (equipButton != null) equipButton.OnButtonReleased -= OnActionPressed;
            if (nextBoatButton != null) nextBoatButton.OnButtonReleased -= OnNextBoatPressed;
            if (prevBoatButton != null) prevBoatButton.OnButtonReleased -= OnPrevBoatPressed;

            if (SkinController != null)
            {
                SkinController.BoatCustomisationScreenActive(false);
                SkinController.OnSkinPurchased -= OnBoatSkinPurchased;
                SkinController.OnSkinColorPurchased -= OnBoatSkinColorPurchased;
            }
        }

        private void OnNextBoatPressed()
        {
            ChangeBoat(1);
        }

        private void OnPrevBoatPressed()
        {
            ChangeBoat(-1);
        }

        private void OnBackOrHomePressed()
        {
            GameController.GetInstance.SetPreviousGameState();
            Close();
        }
        #endregion

        #region Setup & Navigation
        private void SetupCustomisation()
        {
            selectedBoatIndex = SkinController.GetSavedBoatIndex();
            UpdateSelectedBoat(selectedBoatIndex);
            ApplyCameraTransform();
        }

        private void ChangeBoat(int direction)
        {
            var database = SkinController.BoatSkinsDatabase;
            if (database == null || database.BoatSkins == null || database.BoatSkins.Length == 0)
            {
                return;
            }

            int boatCount = database.BoatSkins.Length;
            int newIndex = selectedBoatIndex + direction;
            if (newIndex >= 0 && newIndex < boatCount)
            {
                UpdateSelectedBoat(newIndex);
            }
        }

        public void UpdateSelectedBoat(int index)
        {
            selectedBoatIndex = index;
            selectedColorIndex = 0;
            if (SkinController.GetSavedBoatIndex() == selectedBoatIndex)
            {
                selectedColorIndex = SkinController.GetSavedBoatColorIndex(selectedBoatIndex);
            }

            UpdatePreviewBoatModel();
            UpdateBoatStatsUI();
            RefreshColorSwatches();
            UpdateActionButtonState();
            UpdateNavigationButtons();
        }
        #endregion

        #region Preview & Stats UI
        private void UpdatePreviewBoatModel()
        {
            if (currentPreviewBoat != null)
            {
                currentPreviewBoat.gameObject.SetActive(false);
            }

            if (previewBoatsCache.TryGetValue(selectedBoatIndex, out Boat cachedBoat) && cachedBoat != null)
            {
                currentPreviewBoat = cachedBoat;
                currentPreviewBoat.gameObject.SetActive(true);
            }
            else
            {
                var boatSkinSO = SkinController.GetBoatSkinByIndex(selectedBoatIndex);
                if (boatSkinSO != null && boatSkinSO.BoatPrefab != null)
                {
                    GameObject newBoatObj = Instantiate(boatSkinSO.BoatPrefab);
                    newBoatObj.transform.position = previewBoatPositionOffset;
                    newBoatObj.transform.localRotation = Quaternion.identity;
                    newBoatObj.transform.parent = previewBoatParent;

                    currentPreviewBoat = newBoatObj.GetComponent<Boat>();
                    if (currentPreviewBoat != null)
                    {
                        previewBoatsCache[selectedBoatIndex] = currentPreviewBoat;
                    }
                }
            }

            if (currentPreviewBoat != null)
            {
                int previewColor = selectedColorIndex != -1
                    ? selectedColorIndex
                    : SkinController.GetSavedBoatColorIndex(selectedBoatIndex);
                currentPreviewBoat.SetBoatInCustomisationScreen(selectedBoatIndex, previewColor);
            }
        }

        private void UpdateBoatStatsUI()
        {
            var boatSkinSO = SkinController.GetBoatSkinByIndex(selectedBoatIndex);
            if (boatSkinSO == null) return;

            if (boatNameText != null)
            {
                boatNameText.text = boatSkinSO.Name;
            }
            if (speedBarImages != null && speedBarImages.Length > 0)
            {
                for (int i = 0; i < speedBarImages.Length; i++)
                {
                    if (speedBarImages[i] != null)
                    {
                        speedBarImages[i].sprite = (i < boatSkinSO.SpeedBarFillAmount) ? speedBarFillSprite : speedBarUnfilledSprite;
                    }
                }
            }
            if (speedNeedleTransform != null)
            {
                float needleAngle = Mathf.Lerp(speedNeedleMinAngle, speedNeedleMaxAngle, boatSkinSO.SpeedMeter);
                speedNeedleTransform.localRotation = Quaternion.Euler(0f, 0f, needleAngle);
            }
        }
        #endregion

        #region Color Swatches
        private void RefreshColorSwatches()
        {
            foreach (var boatColorUI in colorUIList)
            {
                if (boatColorUI != null)
                {
                    boatColorUI.gameObject.SetActive(false);
                }
            }

            var boatSkin = SkinController.GetBoatSkinByIndex(selectedBoatIndex);
            if (boatSkin == null || boatSkin.SkinColors == null) return;

            for (int i = 0; i < boatSkin.SkinColors.Length; i++)
            {
                var skinColorData = boatSkin.SkinColors[i];
                var boatSkinColorUI = GetReusableColorUI(i);
                bool isUnlocked = SkinController.IsBoatSkinColorUnlocked(selectedBoatIndex, i);
                boatSkinColorUI.InitSkinColor(this, skinColorData, i, isUnlocked, selectedColorIndex == i);
                boatSkinColorUI.gameObject.SetActive(true);
            }
        }

        public void ApplyBoatColor(int colorIndex)
        {
            selectedColorIndex = colorIndex;

            for (int i = 0; i < colorUIList.Count; i++)
            {
                if (colorUIList[i] != null && colorUIList[i].gameObject.activeSelf)
                {
                    if (i == selectedColorIndex)
                    {
                        colorUIList[i].Select();
                    }
                    else
                    {
                        colorUIList[i].UnSelect();
                    }
                }
            }

            UpdatePreviewBoatModel();
            UpdateActionButtonState();
        }

        private BoatSkinColorUI GetReusableColorUI(int index)
        {
            if (index < colorUIList.Count && colorUIList[index] != null)
            {
                return colorUIList[index];
            }

            var newColorUI = Instantiate(boatSkinColorUIPrefab, boatColorListContainer);
            colorUIList.Add(newColorUI);
            return newColorUI;
        }
        #endregion

        #region Action & Navigation Buttons
        private void UpdateActionButtonState()
        {
            var boatSkin = SkinController.GetBoatSkinByIndex(selectedBoatIndex);

            bool isBoatUnlocked = SkinController.IsBoatSkinUnlocked(selectedBoatIndex);
            int savedBoat = SkinController.GetSavedBoatIndex();
            int savedColor = SkinController.GetSavedBoatColorIndex(selectedBoatIndex);

            bool isColorSelected = selectedColorIndex != -1;
            int activeColor = isColorSelected ? selectedColorIndex : savedColor;
            bool isColorUnlocked = isColorSelected
                ? SkinController.IsBoatSkinColorUnlocked(selectedBoatIndex, selectedColorIndex)
                : true;

            bool isCurrentlyEquipped = (selectedBoatIndex == savedBoat && activeColor == savedColor);

            if (lockObject != null)
            {
                lockObject.SetActive(!isBoatUnlocked);
            }

            if (!isBoatUnlocked)
            {
                currentAction = BoatSelectionAction.PurchaseSkin;
                int cost = boatSkin != null ? boatSkin.CoinCost : 0;
                SetPurchaseButtonState(true, cost);

                if (equipButton != null)
                {
                    equipButton.gameObject.SetActive(false);
                }
            }
            else if (isColorSelected && !isColorUnlocked)
            {
                currentAction = BoatSelectionAction.PurchaseSkinColor;
                int cost = (boatSkin != null && selectedColorIndex >= 0 && selectedColorIndex < boatSkin.SkinColors.Length)
                    ? boatSkin.SkinColors[selectedColorIndex].coinCost
                    : 0;
                SetPurchaseButtonState(true, cost);

                if (equipButton != null)
                {
                    equipButton.gameObject.SetActive(false);
                }
            }
            else
            {
                currentAction = BoatSelectionAction.SelectSkin;
                SetPurchaseButtonState(false);

                if (equipButton != null)
                {
                    equipButton.gameObject.SetActive(true);

                    if (isCurrentlyEquipped)
                    {
                        equipButton.SetInteractable(false);
                        if (equipBtnText != null)
                        {
                            equipBtnText.text = "EQUIPPED";
                        }
                    }
                    else
                    {
                        equipButton.SetInteractable(true);
                        if (equipBtnText != null)
                        {
                            equipBtnText.text = "EQUIP";
                        }
                    }
                }
            }
        }

        private void SetPurchaseButtonState(bool isActive, int cost = 0)
        {
            if (purchaseButtonContainer != null)
            {
                purchaseButtonContainer.SetActive(isActive);
                if (isActive && purchaseBtnText != null)
                {
                    purchaseBtnText.text = $"{cost}";
                    if (purchaseButtonAnimator != null)
                    {
                        purchaseButtonAnimator.Play();
                    }
                }
            }
        }

        public void PlayPurchaseRevealSound()
        {
            AudioController.GetInstance.PlaySound(AudioType.Swoosh);
        }

        private void UpdateNavigationButtons()
        {
            var database = SkinController.BoatSkinsDatabase;
            if (database == null || database.BoatSkins == null || database.BoatSkins.Length == 0)
            {
                if (prevBoatButton != null) prevBoatButton.SetInteractable(false);
                if (nextBoatButton != null) nextBoatButton.SetInteractable(false);
                return;
            }

            int boatCount = database.BoatSkins.Length;
            if (prevBoatButton != null)
            {
                prevBoatButton.SetInteractable(selectedBoatIndex > 0);
            }
            if (nextBoatButton != null)
            {
                nextBoatButton.SetInteractable(selectedBoatIndex < boatCount - 1);
            }
        }

        private void OnActionPressed()
        {
            switch (currentAction)
            {
                case BoatSelectionAction.SelectSkin:
                    int colorToEquip = selectedColorIndex != -1 ? selectedColorIndex : SkinController.GetSavedBoatColorIndex(selectedBoatIndex);
                    SkinController.SetSavedBoatIndex(selectedBoatIndex, colorToEquip);
                    LevelController?.UpdateBoat(selectedBoatIndex, colorToEquip);
                    UpdateActionButtonState();
                    break;

                case BoatSelectionAction.PurchaseSkin:
                    StoreController.BuyBoatWithCoins(selectedBoatIndex);
                    break;

                case BoatSelectionAction.PurchaseSkinColor:
                    StoreController.BuyBoatColorWithCoins(selectedBoatIndex, selectedColorIndex);
                    break;
            }
        }

        private void OnBoatSkinPurchased(int index)
        {
            UpdateSelectedBoat(index);
            LevelController.UpdateBoat(index, 0);
        }

        private void OnBoatSkinColorPurchased(int boatIndex, int colorIndex)
        {
            selectedColorIndex = colorIndex;
            UpdateSelectedBoat(boatIndex);
            LevelController.UpdateBoat(boatIndex, colorIndex);
        }
        #endregion

        #region Camera Logic
        private void ApplyCameraTransform()
        {
            if (currentPreviewBoat == null) return;
            CameraController.GetInstance.SetPreviewCameraEnabled(
                PreviewCameraType.BoatCustomisation,
                true,
                currentPreviewBoat.transform,
                camPositionOffset,
                camRotationOffset
            );
        }
        #endregion
    }
}
