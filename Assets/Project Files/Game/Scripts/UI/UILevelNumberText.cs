using TMPro;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UILevelNumberText : MonoBehaviour
    {
        private const string LEVEL_LABEL = "LEVEL {0}";
        private static UILevelNumberText instance;

        private UIScaleAnimation uIScalableObject;

        private static UIScaleAnimation UIScalableObject => instance.uIScalableObject;
        private static TextMeshProUGUI levelNumberText;

        private static bool IsDisplayed = false;

        private void Awake()
        {
            instance = this;
            levelNumberText = GetComponent<TextMeshProUGUI>();

            uIScalableObject = new UIScaleAnimation(gameObject);
        }

        private void Start()
        {
            UpdateLevelNumber();
        }

        private void OnEnable()
        {
            Debug.Log("[UI Module] Subscribe to level changed event here");
            //GameController.OnLevelChangedEvent += UpdateLevelNumber;
        }

        private void OnDisable()
        {
            Debug.Log("[UI Module] Unsubscribe to level changed event here");
            //GameController.OnLevelChangedEvent -= UpdateLevelNumber;
        }

        public static void Show(bool immediately = false)
        {
            if (IsDisplayed)
                return;

            IsDisplayed = true;

            levelNumberText.enabled = true;
            UIScalableObject.Show(scaleMultiplier: 1.05f, immediately: immediately);
        }

        public static void Hide(bool immediately = false)
        {
            if (!IsDisplayed)
                return;

            if (immediately)
                IsDisplayed = false;

            UIScalableObject.Hide(scaleMultiplier: 1.05f, immediately: immediately, onCompleted: delegate
            {
                IsDisplayed = false;
                levelNumberText.enabled = false;
            });
        }

        private void UpdateLevelNumber()
        {
            Debug.Log("[UI Module] Add level initialization here");
            levelNumberText.text = string.Format(LEVEL_LABEL, "X");
        }

    }
}
