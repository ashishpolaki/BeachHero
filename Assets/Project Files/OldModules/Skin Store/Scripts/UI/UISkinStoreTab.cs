using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SkinStore
{
    [RequireComponent(typeof(Button))]
    public class UISkinStoreTab : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] Image enabledMask;
        [SerializeField] Image disabledMask;
        [SerializeField] Image backImage;

        public TabData Data { get; private set; }
        public bool IsSelected { get; private set; }

        private TabData.SimpleTabDelegate onTabSelected;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClick);
        }

        public void Init(TabData data,TabData.SimpleTabDelegate onTabSelected)
        {
            this.onTabSelected = onTabSelected;

            Data = data;

            text.text = Data.Name;
            backImage.color = IsSelected ? Data.TabActiveColor : Data.TabDisabledColor;
        }

        public void SetSelectedStatus(bool isSelected)
        {
            IsSelected = isSelected;

            enabledMask.enabled = isSelected;
            disabledMask.enabled = !isSelected;

            backImage.color = isSelected ? Data.TabActiveColor : Data.TabDisabledColor;

            button.enabled = !isSelected;
        }

        public void OnButtonClick()
        {
            onTabSelected?.Invoke(Data);
        }
    }
}