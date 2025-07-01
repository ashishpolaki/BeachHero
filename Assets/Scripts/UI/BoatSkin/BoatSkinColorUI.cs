using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatSkinColorUI : MonoBehaviour
    {
        [SerializeField] private GameObject backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button selectButton;

        private BoatColorCustomisationPanel colorCustomisationPanel;
        private int index;

        private void OnEnable()
        {
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }
        private void OnDisable()
        {
            selectButton.onClick.RemoveAllListeners();
        }
        public void InitSkinColor(BoatColorCustomisationPanel _colorCustomisationPanel,BoatSkinColorData skinColorData, int _index)
        {
            index = _index;
            iconImage.color = skinColorData.ShaderColors[0];
            colorCustomisationPanel = _colorCustomisationPanel;
           // backgroundImage.SetActive(isSelected);
        }

        private void OnSelectButtonClicked()
        {
            colorCustomisationPanel.SetBoatColor(index);
            // boatCustomisationUIScreen.SetBoatColor(index);
        }
    }
}
