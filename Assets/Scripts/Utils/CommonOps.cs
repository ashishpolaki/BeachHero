using UnityEngine.Events;
using UnityEngine.UI;

namespace BeachHero
{
    public static class CommonOps
    {
        public static void ButtonRegister(this Button btn, UnityAction action)
        {
            if (!btn)
            {
                DebugUtils.LogError("No Button Exists");
                return;
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }

        public static void ButtonDeRegister(this Button button)
        {
            if (!button)
            {
                DebugUtils.LogError("No Button Exists");
                return;
            }
            button.onClick.RemoveAllListeners();
        }
    }
}