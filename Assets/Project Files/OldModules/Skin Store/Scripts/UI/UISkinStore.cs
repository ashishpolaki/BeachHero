using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bokka.SkinStore
{
    public class UISkinStore : UIPage
    {
        private readonly float PANEL_BOTTOM_OFFSET_Y = -2000f;
        public readonly string STORE_ITEM_POOL_NAME = "StoreItem";

        [BoxGroup("Tab", "Tab")]
        [SerializeField] GameObject tabPrefab;
        [BoxGroup("Tab", "Tab")]
        [SerializeField] Transform tabContainer;

        [BoxGroup("Scroll View", "Scroll View")]
        [SerializeField] RectTransform storeAnimatedPanelRect;
        [BoxGroup("Scroll View", "Scroll View")]
        [SerializeField] Image storePanelBackground;
        [BoxGroup("Scroll View", "Scroll View")]
        [SerializeField] ScrollRect productsScroll;
        [BoxGroup("Scroll View", "Scroll View")]
        [SerializeField] Scrollbar scrollbarHorizontal;
        [BoxGroup("Scroll View", "Scroll View")]
        [SerializeField] Image scrollFadeImage;
        [BoxGroup("Scroll View", "Scroll View")]
        [SerializeField] UISkinItemsGrid storeGrid;
        [BoxGroup("Scroll View", "Scroll View")]
        [SerializeField] ScrollRect scrollView;

        [BoxGroup("Refs", "Refs")]
        [SerializeField] GameObject storeItemPrefab;
        [BoxGroup("Refs", "Refs")]
        [SerializeField] Sprite adsIcon;

        [BoxGroup("Preview", "Preview")]
        [SerializeField] CanvasGroup previewCanvasGroup;
        [BoxGroup("Preview", "Preview")]
        [SerializeField] Image backgroundImage;
        [BoxGroup("Preview", "Preview")]
        [SerializeField] Image previewImage;
        [BoxGroup("Preview", "Preview")]
        [SerializeField] RawImage previewRawImage;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button closeButton;

        [BoxGroup("Currency", "Currency")]
        [SerializeField] CurrencyUIPanelSimple currencyPanel;
        [BoxGroup("Currency", "Currency")]
        [SerializeField] UIFadeAnimation currencyPanelFade;
        [BoxGroup("Currency", "Currency")]
        [SerializeField] Button coinsForAdsButton;
        [BoxGroup("Currency", "Currency")]
        [SerializeField] TextMeshProUGUI coinsForAdsText;
        [BoxGroup("Currency", "Currency")]
        [SerializeField] Image coinsForAdsCurrencyImage;

        [BoxGroup("Safe Area", "Safe Area")]
        [SerializeField] RectTransform safeAreaRectTransform;

        private List<UISkinStoreTab> tabs;
        private Dictionary<TabData, UISkinStoreTab> tabsDictionary;

        private static PoolGeneric<UISkinItem> storeItemPool;
        private static float startedStorePanelRectPositionY;

        private SkinStoreProductContainer SelectedProductContainer { get; set; }
        public Sprite AdsIcon => adsIcon;

        private List<SkinStoreProductContainer> currentPageProductContainers;
        private List<SkinStoreProductContainer> lockedPageProductContainers = new List<SkinStoreProductContainer>();

        private Currency rewardForAdsCurrency;

        private SkinPreview3D storePreview3D;

        public SkinStoreController Controller => SkinStoreController.Instance;

        public override void Init()
        {
            startedStorePanelRectPositionY = storeAnimatedPanelRect.anchoredPosition.y;

            storeItemPool = new PoolGeneric<UISkinItem>(storeItemPrefab, STORE_ITEM_POOL_NAME);

            coinsForAdsText.text = "GET\n" + Controller.CoinsForAdsAmount;

            rewardForAdsCurrency = CurrenciesController.GetCurrency(Controller.CoinsForAdsCurrency);

            currencyPanel.Init();

            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);
        }

        private void OnDestroy()
        {
            PoolManager.DestroyPool(storeItemPool);
        }

        public override void PlayShowAnimation()
        {
            previewCanvasGroup.alpha = 0;
            previewCanvasGroup.DOFade(1, 0.3f);

            InitStoreUI(true);

            storeAnimatedPanelRect.anchoredPosition = storeAnimatedPanelRect.anchoredPosition.SetY(PANEL_BOTTOM_OFFSET_Y);

            storeAnimatedPanelRect.DOAnchoredPosition(new Vector3(storeAnimatedPanelRect.anchoredPosition.x, startedStorePanelRectPositionY + 100f, 0f), 0.4f).SetEasing(Ease.Type.SineInOut).OnComplete(delegate
            {
                storeAnimatedPanelRect.DOAnchoredPosition(new Vector3(storeAnimatedPanelRect.anchoredPosition.x, startedStorePanelRectPositionY, 0f), 0.2f).SetEasing(Ease.Type.SineInOut).OnComplete(() =>
                {
                    UIController.OnPageOpened(this);
                });
            });

            closeButton.transform.localScale = Vector3.zero;
            closeButton.DOScale(1, 0.3f).SetEasing(Ease.Type.SineOut);

            coinsForAdsButton.gameObject.SetActive(AdsManager.Settings.RewardedVideoType != AdProvider.Disable);
            coinsForAdsButton.interactable = true;
            coinsForAdsCurrencyImage.sprite = rewardForAdsCurrency.Icon;

            currencyPanelFade.Show(0.3f, immediately: true);

            AdsManager.HideBanner();
        }

        public void InitStoreUI(bool resetScroll = false)
        {
            // Clear pools
            storeItemPool?.ReturnToPoolEverything(true);

            SelectedProductContainer = Controller.GetSelectedProductContainer();

            TabData tab = Controller.SelectedTabData;
            if (tab.PreviewType == PreviewType.Preview_2D)
            {
                previewRawImage.enabled = false;
                backgroundImage.enabled = true;
                previewImage.enabled = true;
            }
            else
            {
                previewRawImage.enabled = true;
                backgroundImage.enabled = false;
                previewImage.enabled = false;

                if (storePreview3D != null)
                    Destroy(storePreview3D.gameObject);

                Debug.Log(tab.PreviewPrefab);
                Debug.Log(tab.PreviewPrefab.GetComponent<SkinPreview3D>());

                storePreview3D = Instantiate(tab.PreviewPrefab).GetComponent<SkinPreview3D>();
                storePreview3D.Init();
                storePreview3D.SpawnProduct(SelectedProductContainer.ProductData);

                previewRawImage.texture = storePreview3D.Texture;
            }

            previewImage.sprite = SelectedProductContainer.ProductData.Preview2DSprite;
            backgroundImage.color = Controller.SelectedTabData.BackgroundColor;
            backgroundImage.sprite = Controller.SelectedTabData.BackgroundImage;

            storeGrid.Init(Controller, Controller.GetProducts(Controller.SelectedTabData), SelectedProductContainer.ProductData.UniqueId);

            if (resetScroll)
                scrollView.normalizedPosition = Vector2.up;

            UpdateCurrentPage(true);

            for (int i = 0; i < tabs.Count; i++)
            {
                tabs[i].SetSelectedStatus(tabs[i].Data == Controller.SelectedTabData);
            }
        }

        private void UpdateCurrentPage(bool redrawStorePage)
        {
            currentPageProductContainers = Controller.GetProducts(Controller.SelectedTabData);

            lockedPageProductContainers.Clear();

            for (int i = 0; i < currentPageProductContainers.Count; i++)
            {
                SkinStoreProductContainer container = currentPageProductContainers[i];

                if (!container.IsUnlocked && !container.ProductData.IsDummy)
                {
                    lockedPageProductContainers.Add(container);
                }
            }

            if (redrawStorePage)
            {
                storeGrid.UpdateItems(SelectedProductContainer.ProductData.UniqueId);
            }

            storePanelBackground.color = Controller.SelectedTabData.PanelColor;
            scrollFadeImage.color = Controller.SelectedTabData.PanelColor;

            productsScroll.enabled = currentPageProductContainers.Count > 6;
            scrollbarHorizontal.gameObject.SetActive(currentPageProductContainers.Count > 6);
            scrollFadeImage.gameObject.SetActive(currentPageProductContainers.Count > 6);

            bool isEven = currentPageProductContainers.Count % 2 == 0;

            if (isEven)
            {
                scrollbarHorizontal.numberOfSteps = (currentPageProductContainers.Count - 6) / 2 + 1;
            }
            else
            {
                scrollbarHorizontal.numberOfSteps = (currentPageProductContainers.Count - 6) / 2 + 2;
            }
        }

        public override void PlayHideAnimation()
        {
            closeButton.DOScale(0, 0.3f).SetEasing(Ease.Type.SineIn);

            if (storePreview3D != null)
            {
                Destroy(storePreview3D.gameObject);
            }

            previewCanvasGroup.DOFade(0, 0.3f);

            storeAnimatedPanelRect.DOAnchoredPosition(new Vector3(storeAnimatedPanelRect.anchoredPosition.x, startedStorePanelRectPositionY + 100f, 0f), 0.2f).SetEasing(Ease.Type.SineInOut).OnComplete(delegate
            {
                storeAnimatedPanelRect.DOAnchoredPosition(new Vector3(storeAnimatedPanelRect.anchoredPosition.x, PANEL_BOTTOM_OFFSET_Y, 0f), 0.4f).SetEasing(Ease.Type.SineInOut).OnComplete(delegate
                {
                    UIController.OnPageClosed(this);

                    AdsManager.ShowBanner();
                });
            });

            currencyPanelFade.Hide(0.3f);
        }

        public void InitTabs(TabData.SimpleTabDelegate OnTabClicked)
        {
            tabsDictionary = new Dictionary<TabData, UISkinStoreTab>();
            tabs = new List<UISkinStoreTab>();

            TabData[] tabsData = Controller.Database.Tabs;
            for (int i = 0; i < tabsData.Length; i++)
            {
                if (tabsData[i].IsActive)
                {
                    GameObject tempTabObject = Instantiate(tabPrefab, tabContainer);
                    tempTabObject.transform.ResetLocal();
                    tempTabObject.SetActive(true);

                    UISkinStoreTab skinStoreTab = tempTabObject.GetComponent<UISkinStoreTab>();
                    skinStoreTab.Init(tabsData[i], OnTabClicked);

                    tabs.Add(skinStoreTab);

                    tabsDictionary.Add(tabsData[i], skinStoreTab);
                }
            }
        }

        public void SetSelectedTab(TabData tabData)
        {
            foreach (var tab in tabs)
            {
                tab.SetSelectedStatus(tab.Data == tabData);
            }

            InitStoreUI(true);
        }

        public void GetCoinsForAdsButton()
        {
            coinsForAdsButton.interactable = false;

            AdsManager.ShowRewardBasedVideo((bool success) =>
            {
                if (success)
                {
                    FloatingCloud.SpawnCurrency(rewardForAdsCurrency.CurrencyType.ToString(), coinsForAdsText.rectTransform, currencyPanel.RectTransform, 20, "", () =>
                    {
                        CurrenciesController.Add(rewardForAdsCurrency.CurrencyType, Controller.CoinsForAdsAmount);

                        UpdateCurrentPage(true);

                        coinsForAdsButton.interactable = true;
                    });
                }
                else
                {
                    coinsForAdsButton.interactable = true;
                }
            });
        }

        public void CloseButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            UIController.HidePage<UISkinStore>();
        }
    }
}