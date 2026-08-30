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
        // References
        [SerializeField] private BoatSkinDatabaseSO boatSkinsDatabase;
        [SerializeField] private BoatSkinColorUI boatSkinColorUIPrefab;
        [SerializeField] private Transform boatColorListContainer;
        [SerializeField] private Transform previewBoatParent;
        [SerializeField] private GameObject lockObject;

        //UI
        [SerializeField] private UIButton backButton;
        [SerializeField] private UIButton purchaseButton;
        [SerializeField] private UIButton equipButton;
        [SerializeField] private UIButton nextBoatButton;
        [SerializeField] private UIButton prevBoatButton;
        [SerializeField] private TextMeshProUGUI purchaseBtnText;
        [SerializeField] private TextMeshProUGUI equipBtnText;
        [SerializeField] private TextMeshProUGUI boatNameText;

        [Header("Speed Gauge Settings")]
        [SerializeField] private Image speedBarFill;
        [SerializeField] private Transform speedNeedleTransform;
        [SerializeField] private float speedNeedleMinAngle = 100f;
        [SerializeField] private float speedNeedleMaxAngle = -100f;

        [Header("Camera Settings")]
        [SerializeField] private Vector3 previewBoatPositionOffset = new Vector3(50f, 0f, 0f);
        [SerializeField] private Vector3 camPositionOffset = new Vector3(2.55f, 2.82f, 4.87f);
        [SerializeField] private Vector3 camRotationOffset = new Vector3(17.191f, 205.917f, 357.561f);
        #endregion

        #region Private Variables
        private BoatSelectionAction boatSelectionAction = BoatSelectionAction.SelectSkin;
        private int selectedBoatIndex = -1;
        private int selectedColorIndex = 0;
        private List<BoatSkinColorUI> colorUIList = new List<BoatSkinColorUI>();
        private Dictionary<int, Boat> previewBoatsCache = new Dictionary<int, Boat>();
        private Boat currentPreviewBoat;
        #endregion

        #region Override Methods
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            AddListeners();
            SetupCustomisation();
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
        }
        #endregion

        #region Button & Event Listeners
        private void AddListeners()
        {
            if (backButton != null) backButton.OnButtonReleased += (OnBackOrHomePressed);
            if (purchaseButton != null) purchaseButton.OnButtonReleased += (OnPurchasePressed);
            if (equipButton != null) equipButton.OnButtonReleased += (OnPurchasePressed);
            if (nextBoatButton != null) nextBoatButton.OnButtonReleased += (OnNextBoatPressed);
            if (prevBoatButton != null) prevBoatButton.OnButtonReleased += (OnPrevBoatPressed);

            var skinController = GameController.GetInstance.SkinController;
            if (skinController != null)
            {
                skinController.BoatCustomisationScreenActive(true);
                skinController.OnSkinPurchased += BoatSkinPurchased;
                skinController.OnSkinColorPurchased += BoatSkinColorPurchased;
            }
        }

        private void RemoveListeners()
        {
            if (backButton != null) backButton.OnButtonReleased -= (OnBackOrHomePressed);
            if (purchaseButton != null) purchaseButton.OnButtonReleased -= (OnPurchasePressed);
            if (equipButton != null) equipButton.OnButtonReleased -= (OnPurchasePressed);
            if (nextBoatButton != null) nextBoatButton.OnButtonReleased -= (OnNextBoatPressed);
            if (prevBoatButton != null) prevBoatButton.OnButtonReleased -= (OnPrevBoatPressed);

            var skinController = GameController.GetInstance.SkinController;
            if (skinController != null)
            {
                skinController.BoatCustomisationScreenActive(false);
                skinController.OnSkinPurchased -= BoatSkinPurchased;
                skinController.OnSkinColorPurchased -= BoatSkinColorPurchased;
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
        private void UpdateNavigationButtons()
        {
            var database = GameController.GetInstance.SkinController.BoatSkinsDatabase;
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
        #endregion

        #region Camera Logic
        private void ApplyCameraTransform()
        {
            if (currentPreviewBoat == null) return;
            CameraController.GetInstance.SetPreviewCameraEnabled(PreviewCameraType.BoatCustomisation, true, currentPreviewBoat.transform, camPositionOffset, camRotationOffset);
        }
        #endregion

        #region Setup/Init Boat
        private void SetupCustomisation()
        {
            // Start from currently equipped boat
            selectedBoatIndex = GameController.GetInstance.SkinController.GetSavedBoatIndex();
            UpdateSelectedBoat(selectedBoatIndex);
            ApplyCameraTransform();
        }

        private void ChangeBoat(int direction)
        {
            var database = GameController.GetInstance.SkinController.BoatSkinsDatabase;
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
            selectedColorIndex = 0; // Reset color index when changing boats
            if (GameController.GetInstance.SkinController.GetSavedBoatIndex() == selectedBoatIndex)
            {
                selectedColorIndex = GameController.GetInstance.SkinController.GetSavedBoatColorIndex(selectedBoatIndex);
            }
            HighlightSelectedBoat();
            ShowAvailableColors();
            UpdateSelectionState();
            UpdateNavigationButtons();
        }

        private void HighlightSelectedBoat()
        {
            UpdatePreviewBoat();

            // Set boat stats in UI panel
            var boatSkinSO = GameController.GetInstance.SkinController.GetBoatSkinByIndex(selectedBoatIndex);
            if (boatSkinSO != null)
            {
                if (boatNameText != null)
                {
                    boatNameText.text = boatSkinSO.Name;
                }
                if (speedBarFill != null)
                {
                    speedBarFill.fillAmount = boatSkinSO.SpeedMeter;
                }
                if(speedNeedleTransform != null)
                {
                    float needleAngle = Mathf.Lerp(speedNeedleMinAngle, speedNeedleMaxAngle, boatSkinSO.SpeedMeter);
                    speedNeedleTransform.localRotation = Quaternion.Euler(0f, 0f, needleAngle);
                }
            }
        }

        private void UpdatePreviewBoat()
        {
            // Turn off currently active preview boat
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
                var boatSkinSO = GameController.GetInstance.SkinController.GetBoatSkinByIndex(selectedBoatIndex);
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
                currentPreviewBoat.SetBoatInCustomisationScreen(selectedBoatIndex, selectedColorIndex);
            }
        }
        #endregion

        #region Boat Colors
        private void ShowAvailableColors()
        {
            // Deactivate all existing color UIs
            foreach (var boatColorUI in colorUIList)
            {
                if (boatColorUI != null)
                {
                    boatColorUI.gameObject.SetActive(false);
                }
            }

            var skinController = GameController.GetInstance.SkinController;
            var boatSkin = skinController.GetBoatSkinByIndex(selectedBoatIndex);
            if (boatSkin == null || boatSkin.SkinColors == null) return;

            for (int i = 0; i < boatSkin.SkinColors.Length; i++)
            {
                var skinColorData = boatSkin.SkinColors[i];
                var boatSkinColorUI = GetReusableColorUI(i);
                bool isUnlocked = skinController.IsBoatSkinColorUnlocked(selectedBoatIndex, i);
                boatSkinColorUI.InitSkinColor(this, skinColorData, i, isUnlocked, selectedColorIndex == i);
                boatSkinColorUI.gameObject.SetActive(true);
            }
        }

        public void ApplyBoatColor(int colorIndex)
        {
            selectedColorIndex = colorIndex;

            // Update color swatch selection visual
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

            HighlightSelectedBoat();
            UpdateSelectionState();
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

        #region Purchase & Equip Logic
        private void UpdateSelectionState()
        {
            var skinController = GameController.GetInstance.SkinController;
            var boatSkin = skinController.GetBoatSkinByIndex(selectedBoatIndex);

            bool isBoatUnlocked = skinController.IsBoatSkinUnlocked(selectedBoatIndex);
            bool isColorUnlocked = skinController.IsBoatSkinColorUnlocked(selectedBoatIndex, selectedColorIndex);
            int savedBoat = skinController.GetSavedBoatIndex();
            int savedColor = skinController.GetSavedBoatColorIndex(selectedBoatIndex);

            bool isCurrentlyEquipped = (selectedBoatIndex == savedBoat && selectedColorIndex == savedColor);

            if (lockObject != null)
            {
                lockObject.SetActive(!isBoatUnlocked);
            }

            if (!isBoatUnlocked)
            {
                // 1. Boat is locked -> Show Buy Boat Button, Hide Equip Button
                boatSelectionAction = BoatSelectionAction.PurchaseSkin;

                if (purchaseButton != null)
                {
                    purchaseButton.gameObject.SetActive(true);
                    if (purchaseBtnText != null)
                    {
                        int cost = boatSkin != null ? boatSkin.CoinCost : 0;
                        purchaseBtnText.text = $"{cost}";
                    }
                }

                if (equipButton != null)
                {
                    equipButton.gameObject.SetActive(false);
                }
            }
            else if (!isColorUnlocked)
            {
                // 2. Boat is unlocked, but selected color is locked -> Show Buy Color Button, Hide Equip Button
                boatSelectionAction = BoatSelectionAction.PurchaseSkinColor;

                if (purchaseButton != null)
                {
                    purchaseButton.gameObject.SetActive(true);
                    if (purchaseBtnText != null)
                    {
                        int cost = (boatSkin != null && selectedColorIndex >= 0 && selectedColorIndex < boatSkin.SkinColors.Length)
                            ? boatSkin.SkinColors[selectedColorIndex].coinCost
                            : 0;
                        purchaseBtnText.text = $"{cost}";
                    }
                }

                if (equipButton != null)
                {
                    equipButton.gameObject.SetActive(false);
                }
            }
            else
            {
                // 3. Both boat and selected color are unlocked -> Hide Purchase Button, Show Equip Button
                boatSelectionAction = BoatSelectionAction.SelectSkin;

                if (purchaseButton != null)
                {
                    purchaseButton.gameObject.SetActive(false);
                }

                if (equipButton != null)
                {
                    equipButton.gameObject.SetActive(true);

                    if (isCurrentlyEquipped)
                    {
                        // Already equipped -> disabled
                        equipButton.SetInteractable(false);
                        if (equipBtnText != null)
                        {
                            equipBtnText.text = "EQUIPPED";
                        }
                    }
                    else
                    {
                        // Unlocked but not currently equipped -> interactable
                        equipButton.SetInteractable(true);
                        if (equipBtnText != null)
                        {
                            equipBtnText.text = "EQUIP";
                        }
                    }
                }
            }
        }

        private void OnPurchasePressed()
        {
            var skinController = GameController.GetInstance.SkinController;
            var storeController = GameController.GetInstance.StoreController;

            switch (boatSelectionAction)
            {
                case BoatSelectionAction.SelectSkin:
                    skinController.SetSavedBoatIndex(selectedBoatIndex, selectedColorIndex);
                    GameController.GetInstance.LevelController?.UpdateBoat(selectedBoatIndex, selectedColorIndex);
                    UpdateSelectionState();
                    break;

                case BoatSelectionAction.PurchaseSkin:
                    storeController.BuyBoatWithCoins(selectedBoatIndex);
                    UpdateSelectedBoat(selectedBoatIndex);
                    break;

                case BoatSelectionAction.PurchaseSkinColor:
                    storeController.BuyBoatColorWithCoins(selectedBoatIndex, selectedColorIndex);
                    UpdateSelectedBoat(selectedBoatIndex);
                    break;
            }
        }

        private void BoatSkinPurchased(int index)
        {
            selectedBoatIndex = index;
            selectedColorIndex = 0;
            UpdateSelectedBoat(index);
        }

        private void BoatSkinColorPurchased(int boatIndex, int colorIndex)
        {
            selectedBoatIndex = boatIndex;
            selectedColorIndex = colorIndex;
            ShowAvailableColors();
            ApplyBoatColor(colorIndex);
        }
        #endregion

        #region Save Boat Image
        //#if UNITY_EDITOR
        //        [SerializeField] private RenderTexture renderTexture1;
        //        public bool onValiate;
        //        private void OnValidate()
        //        {
        //            if (onValiate)
        //            {
        //                SaveRenderTextureAsSprite(renderTexture1, "Boat");
        //            }
        //        }

        //        public Sprite SaveRenderTextureAsSprite(RenderTexture renderTexture, string fileName)
        //        {
        //            RenderTexture previous = RenderTexture.active;
        //            RenderTexture.active = renderTexture;

        //            Texture2D texture = new Texture2D(
        //                renderTexture.width,
        //                renderTexture.height,
        //                TextureFormat.RGBA32,
        //                false
        //            );

        //            texture.ReadPixels(
        //                new Rect(0, 0, renderTexture.width, renderTexture.height),
        //                0,
        //                0
        //            );

        //            texture.Apply();

        //            RenderTexture.active = previous;

        //            string folderPath = "Assets/Sprites";

        //            if (!AssetDatabase.IsValidFolder(folderPath))
        //            {
        //                AssetDatabase.CreateFolder("Assets", "Sprites");
        //            }

        //            string assetPath = $"{folderPath}/{fileName}.png";

        //            byte[] pngData = texture.EncodeToPNG();
        //            File.WriteAllBytes(assetPath, pngData);

        //            AssetDatabase.Refresh();

        //            // Get the imported texture
        //            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

        //            // Make sure Unity imports it as a Sprite
        //            TextureImporter importer =
        //                AssetImporter.GetAtPath(assetPath) as TextureImporter;

        //            if (importer != null)
        //            {
        //                importer.textureType = TextureImporterType.Sprite;
        //                importer.spriteImportMode = SpriteImportMode.Single;

        //                AssetDatabase.ImportAsset(
        //                    assetPath, ImportAssetOptions.ForceUpdate);
        //            }

        //            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        //            Object.DestroyImmediate(texture);
        //            return sprite;
        //        }
        //#endif
        #endregion

    }
}
