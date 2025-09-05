using UnityEngine;

namespace BeachHero
{
    public class DebugUtils
    {
        public static void Log(string message, Object context = null)
        {
#if ENABLE_DEBUG
            Debug.Log(message, context);
#endif
        }
        public static void LogWarning(string message, Object context = null)
        {
#if ENABLE_DEBUG
            Debug.LogWarning(message, context);
#endif
        }

        public static void LogError(string message, Object context = null)
        {
#if ENABLE_DEBUG
            Debug.LogError(message, context);
#endif
        }

        public static void LogException(System.Exception exception, Object context = null)
        {
#if ENABLE_DEBUG
            Debug.LogException(exception, context);
#endif
        }

        public static void LogFormat(string format, params object[] args)
        {
#if ENABLE_DEBUG
            Debug.LogFormat(format, args);
#endif
        }

        //Debug break
        public static void Break()
        {
#if ENABLE_DEBUG
            Debug.Break();
#endif
        }
    }
}
