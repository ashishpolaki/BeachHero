using UnityEngine;

namespace Bokka
{
    public static class CommonOps
    {
        public static void Log(this string msg, GameObject go = null)
        {
            Debug.Log($"<color=white>[Bokka]:{msg}</color>", go);
        }
        public static void LogError(this string msg, GameObject go = null)
        {
            Debug.Log($"<color=red>[Bokka]:{msg}</color>", go);
        }
        public static void LogWarning(this string msg, GameObject go = null)
        {
            Debug.Log($"<color=yellow>[Bokka]:{msg}</color>", go);
        }
    }
}