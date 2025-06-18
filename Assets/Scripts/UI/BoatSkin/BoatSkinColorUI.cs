using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatSkinColorUI : MonoBehaviour
    {
        [SerializeField] private GameObject backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button selectButton;

        private BoatCustomisationUIScreen boatCustomisationUIScreen;
        private int index;

        private void OnEnable()
        {
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        private void OnDisable()
        {
            selectButton.onClick.RemoveAllListeners();
        }

        public void InitSkinColor(BoatCustomisationUIScreen _boatCustomisationUIScreen, BoatSkinColorData skinColorData, int _index)
        {
            boatCustomisationUIScreen = _boatCustomisationUIScreen;
            index = _index;
            iconImage.color = skinColorData.ShaderColors[0];
           // backgroundImage.SetActive(isSelected);
        }

        private void OnSelectButtonClicked()
        {
            boatCustomisationUIScreen.SetBoatColor(index);
        }
    }
}
