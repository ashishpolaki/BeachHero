using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatSkinColorUI : MonoBehaviour
    {
        [SerializeField] private GameObject backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button selectButton;

        private void OnEnable()
        {
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        private void OnDisable()
        {
            selectButton.onClick.RemoveAllListeners();
        }

        public void SetSkinColor(BoatCustomisationUIScreen boatCustomisationUIScreen, BoatSkinColorData skinColorData, int index, bool isSelected)
        {
            iconImage.sprite = skinColorData.sprite;
            backgroundImage.SetActive(isSelected);
        }

        private void OnSelectButtonClicked()
        {
            var boatSkinUI = GetComponentInParent<BoatSkinUI>();
            if (boatSkinUI != null)
            {
              //  boatSkinUI.OnColorSelected(iconImage.sprite);
            }
        }
    }
}
