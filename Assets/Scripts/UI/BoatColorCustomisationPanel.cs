using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatColorCustomisationPanel : MonoBehaviour
    {
        [SerializeField] private BoatSkinColorUI boatSkinColorUIPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform content;
        [SerializeField] private Image boatImage;
        [SerializeField] private Button gameCurrencyBuyButton;
        [SerializeField] private Button adBuyButton;
        [SerializeField] private TextMeshProUGUI gameCurrencyText;
        [SerializeField] private TextMeshProUGUI adText;
        private List<BoatSkinColorUI> boatSkinColorUIList = new List<BoatSkinColorUI>();
        private int currentBoatIndex;
        private int currentBoatColorIndex;

        public void AddListeners()
        {
            closeButton.onClick.AddListener(Close);
            gameCurrencyBuyButton.onClick.AddListener(OnGameCurrencyBuyButtonClicked);
            adBuyButton.onClick.AddListener(OnAdBuyButtonClicked);
        }
        public void RemoveListeners()
        {
            closeButton.onClick.RemoveAllListeners();
            gameCurrencyBuyButton.onClick.RemoveAllListeners();
            adBuyButton.onClick.RemoveAllListeners();
        }
        public void Close()
        {
            gameObject.SetActive(false);
        }
        public void Open(int index, int colorIndex)
        {
            gameObject.SetActive(true);
            currentBoatIndex = index;
            SetBoatColor(colorIndex);
            ShowCurrentBoatSkinColors();
        }

        private void OnGameCurrencyBuyButtonClicked()
        {
            var boatSkin = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentBoatIndex);
            //if (boatSkin.IsPurchased)
            //{
            //    SetBoatColor(boatSkin.CurrentColorIndex);
            //    Close();
            //}
            //else
            //{
            //    GameController.GetInstance.SkinController.BuyBoatSkinWithGameCurrency(currentBoatIndex);
            //}
        }

        private void OnAdBuyButtonClicked()
        {
            var boatSkin = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentBoatIndex);
            if (boatSkin.SkinColors[currentBoatColorIndex].isAds)
            {
                GameController.GetInstance.SkinController.SetBoatColorAdsCount(currentBoatIndex, currentBoatColorIndex);
            }
        }

        public void SetBoatColor(int colorIndex)
        {
            currentBoatColorIndex = colorIndex;
            var boatSkin = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentBoatIndex);
            boatImage.sprite = boatSkin.SkinColors[colorIndex].sprite;

            bool isDefaultColor = boatSkin.SkinColors[colorIndex].isDefault;

            //Set the buttons Text
            bool isAds = boatSkin.SkinColors[currentBoatColorIndex].isAds;
            if (isAds)
            {
                int currentAds = GameController.GetInstance.SkinController.GetBoatColorAdsCount(currentBoatIndex, currentBoatColorIndex);
                adText.text = $"{currentAds}/{boatSkin.SkinColors[currentBoatColorIndex].adsRequired}";
            }
            adBuyButton.gameObject.SetActive(isAds && !isDefaultColor);

            bool isGameCurrency = boatSkin.SkinColors[currentBoatColorIndex].isGameCurrency;
            if (isGameCurrency)
            {
                gameCurrencyText.text = $"{boatSkin.SkinColors[currentBoatColorIndex].inGameCurrencyCost}";
            }
            gameCurrencyBuyButton.gameObject.SetActive(isGameCurrency && !isDefaultColor);
        }

        private void ShowCurrentBoatSkinColors()
        {
            var boatSkin = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentBoatIndex);
            // Deactivate all existing color UIs
            foreach (var ui in boatSkinColorUIList)
                ui.gameObject.SetActive(false);

            // Set up color UIs for current skin
            for (int i = 0; i < boatSkin.SkinColors.Length; i++)
            {
                var skinColorData = boatSkin.SkinColors[i];
                var boatSkinColorUI = GetBoatSkinColorUI();
                int index = i;
                boatSkinColorUI.InitSkinColor(this, skinColorData, index);
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
            var boatSkinColorObj = Instantiate(boatSkinColorUIPrefab, content);
            boatSkinColorUIList.Add(boatSkinColorObj);
            return boatSkinColorObj;
        }
    }
}
