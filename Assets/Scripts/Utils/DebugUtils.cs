using UnityEngine;

namespace BeachHero
{
    public class DebugUtils
    {
        public static void Log(string message, Object context = null)
        {
#if DEBUG
            Debug.Log(message, context);
#endif
        }
        public static void LogWarning(string message, Object context = null)
        {
#if DEBUG
            Debug.LogWarning(message, context);
#endif
        }

        public static void LogError(string message, Object context = null)
        {
#if DEBUG
            Debug.LogError(message, context);
#endif
        }

        public static void LogException(System.Exception exception, Object context = null)
        {
#if DEBUG
            Debug.LogException(exception, context);
#endif
        }

        public static void LogFormat(string format, params object[] args)
        {
#if DEBUG
            Debug.LogFormat(format, args);
#endif
        }

        //Debug break
        public static void Break()
        {
#if DEBUG
            Debug.Break();
#endif
        }
    }
}
