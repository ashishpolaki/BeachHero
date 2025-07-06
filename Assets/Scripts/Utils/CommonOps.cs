using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BeachHero
{
    public static class CommonOps
    {
        public static void Log(this string msg, GameObject go = null)
        {
            Debug.Log($"<color=white>[BeachHero]:{msg}</color>", go);
        }
        public static void LogError(this string msg, GameObject go = null)
        {
            Debug.Log($"<color=red>[BeachHero]:{msg}</color>", go);
        }
        public static void LogWarning(this string msg, GameObject go = null)
        {
            Debug.Log($"<color=yellow>[BeachHero]:{msg}</color>", go);
        }

        public static void ButtonRegister(this Button btn, UnityAction action)
        {
            if (!btn)
            {
                $"No Button Exists".LogError();
                return;
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }
}