using UnityEngine;
using UnityEngine.UI;

namespace Bokka
{
    public abstract class SettingsButtonBase : MonoBehaviour
    {
        public RectTransform RectTransform { get; private set; }
        public Button Button { get; private set; }

        private void Awake()
        {
            RectTransform = (RectTransform)transform;

            Button = GetComponent<Button>();
            Button.onClick.AddListener(OnClick);

            Init();
        }

        public abstract void Init();
        public abstract void OnClick();
    }
}