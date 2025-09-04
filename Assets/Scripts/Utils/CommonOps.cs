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
            btn.onClick.AddListener(action);
        }

        public static void ButtonDeRegisterAll(this Button button)
        {
            if (!button)
            {
                DebugUtils.LogError("No Button Exists");
                return;
            }
            button.onClick.RemoveAllListeners();
        }
        public static void ButtonDeRegister(this Button button, UnityAction action)
        {
            if (!button)
            {
                DebugUtils.LogError("No Button Exists");
                return;
            }
            button.onClick.RemoveListener(action);
        }
    }
}