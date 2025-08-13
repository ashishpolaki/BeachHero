using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Haptics
{
    public static class Most_HapticFeedback
    {
        [Serializable]
        [Tooltip("Each element = one pulse")]
        public struct CustomHapticPattern
        {
            [Tooltip("IOS Pulse data")]
            public IOS_Haptic[] IOS_HapticPattern;
            [Tooltip("Android Pulse data")]
            public Android_Haptic[] Android_HapticPattern;

            // class constructor
            public CustomHapticPattern(IOS_Haptic[] iosHaptic, Android_Haptic[] androidHaptic)
            {
                IOS_HapticPattern = iosHaptic;
                Android_HapticPattern = androidHaptic;
            }
        }

        [Serializable]
        public struct IOS_Haptic
        {
            // iOS haptic data
            [Tooltip("Delay before starting this pulse in milliseconds")]
            public float Delay;
            [Tooltip("Haptic type of this pulse")]
            public HapticTypes PulseType;

            // class constructor
            public IOS_Haptic(HapticTypes type, float delay)
            {
                Delay = delay;
                PulseType = type;
            }
        }

        [Serializable]
        public struct Android_Haptic
        {
            // Android haptic data
            [Tooltip("Delay before starting this pulse in milliseconds")]
            public long Delay;
            [Tooltip("Pulse time in milliseconds")]
            public long PulseTime;
            [Tooltip("vibration Strength of the pulse\ninteger (0-255)")]
            public int PulseStrength;

            // class constructor
            public Android_Haptic(long delay, long pattern, int amplitudes)
            {
                Delay = delay;
                PulseTime = pattern;
                PulseStrength = amplitudes;
            }
        }

        public enum HapticTypes
        {
            // These names are exactly the same as the objective c haptic feedback api (limited only to these feedbacks)
            // so i have used these names on android as well...
            // it's limited on ios but you can create unlimited custom patterns on android(check IOSDefaultHapticsToAndroidPatterns() line 49)
            Selection,    // case 0 // IOS 10+
            Success,      // case 1 // IOS 10+
            Warning,      // case 2 // IOS 10+
            Failure,      // case 3 // IOS 10+
            LightImpact,  // case 4 // IOS 10+
            MediumImpact, // case 5 // IOS 10+
            HeavyImpact,  // case 6 // IOS 10+
            RigidImpact,  // case 7 // IOS 13+ <<
            SoftImpact,   // case 8 // IOS 13+ <<
        }
#if UNITY_ANDROID && !UNITY_EDITOR  // This function reverse iOS default haptics enum to android haptic patterns (Android only)
        static void IOSDefaultHapticsToAndroidPatterns( 
            HapticTypes type, out long[] pattern, out int[] amplitudes)
        {
            // using 'pattern' and 'amplitudes' create android custom haptic feedback
            // pattern is the Timings for vibration pulses in milliseconds + delay betweem >>> Format [delay, vibrate, delay, vibrate, ...]
            // amplitudes is the strength of the pulse >>> integer (0-255)
            switch (type) 
            {
                case HapticTypes.Selection:
                    pattern = new long[] { 0, 20 };
                    amplitudes = new int[] { 0, 80 };
                    break;
                case HapticTypes.Success:
                    pattern = new long[] { 0, 100, 50, 100 };
                    amplitudes = new int[] { 0, 150, 0, 150 };
                    break;
                case HapticTypes.Warning:
                    pattern = new long[] { 0, 200 };
                    amplitudes = new int[] { 0, 200 };
                    break;
                case HapticTypes.Failure:
                    pattern = new long[] { 0, 40, 40, 40 };
                    amplitudes = new int[] { 0, 255, 0, 255 };
                    break;
                case HapticTypes.LightImpact:
                    pattern = new long[] { 0, 50 };
                    amplitudes = new int[] { 0, 100 };
                    break;
                case HapticTypes.MediumImpact:
                    pattern = new long[] { 0, 100 };
                    amplitudes = new int[] { 0, 180 };
                    break;
                case HapticTypes.HeavyImpact:
                    pattern = new long[] { 0, 200 };
                    amplitudes = new int[] { 0, 255 };
                    break;
                case HapticTypes.RigidImpact:
                    pattern = new long[] { 0, 25 };
                    amplitudes = new int[] { 0, 255 };
                    break;
                case HapticTypes.SoftImpact:
                    pattern = new long[] { 0, 80 };
                    amplitudes = new int[] { 0, 80 };
                    break;
                default:
                    pattern = new long[] { 0, 100 };
                    amplitudes = new int[] { 0, 150 };
                    break;
            }
        }
#endif

        static bool _hapticsEnabled = true;
        static bool _initialized = false;
        static AndroidJavaObject _androidVibrator;
        static AndroidJavaClass _vibrationEffectClass;
        static int _androidApiLevel;
        static float _lastHapticTime;
        static float _hapticCooldown = 0.1f; // 100ms minimum between haptics

        // iOS Native Plugin Interface
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void MOST_HapticFeedback(int type);
#endif

        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            InitializeAndroid();
#endif

#if UNITY_IOS && !UNITY_EDITOR
            Debug.Log("Initializing iOS haptics");
#endif

            _initialized = true;
        }

        public static void GenerateWithCooldown(HapticTypes type, float cooldown = -1f)
        {
            float timeSinceLast = Time.unscaledTime - _lastHapticTime;
            float requiredCooldown = cooldown > 0 ? cooldown : _hapticCooldown;

            if (timeSinceLast >= requiredCooldown)
            {
                Generate(type);
                _lastHapticTime = Time.unscaledTime;
            }
        }

        public static IEnumerator GeneratePattern(CustomHapticPattern hapticPattern)
        {
#if UNITY_IOS && !UNITY_EDITOR
            foreach (IOS_Haptic haptic in hapticPattern.IOS_HapticPattern)
            {
                yield return new WaitForSeconds(haptic.Delay / 100f);
                GenerateIOS(haptic.PulseType);
            }

#elif UNITY_ANDROID && !UNITY_EDITOR
            List<long> pattern = new(); List<int> amp = new();
            foreach (Android_Haptic haptic in hapticPattern.Android_HapticPattern)
            {
                pattern.Add(haptic.Delay);
                amp.Add(0);
                pattern.Add(haptic.PulseTime);
                amp.Add(haptic.PulseStrength);
            }
            GenerateAndroid(pattern.ToArray(),amp.ToArray());
#endif
            yield break;
        }

        public static void Generate(HapticTypes type)
        {
            if (!_hapticsEnabled || !_initialized) return;

#if UNITY_IOS && !UNITY_EDITOR
            GenerateIOS(type);
#elif UNITY_ANDROID && !UNITY_EDITOR
            IOSDefaultHapticsToAndroidPatterns(type, out long[] pattern, out int[] amp);
            GenerateAndroid(pattern, amp);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        static void InitializeAndroid()
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    _androidVibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    _androidApiLevel = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
                
                    if (_androidApiLevel >= 26) // Oreo 8.0+
                    {
                        _vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                    }
                }
                Debug.Log("Android haptics initialized. API Level: " + _androidApiLevel);
            }
            catch (Exception e)
            {
                Debug.LogError("Android haptics initialization failed: " + e.Message);
            }
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        static void GenerateIOS(HapticTypes type)
        {
            try
            {
                MOST_HapticFeedback((int)type);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to generate iOS haptic {type}: {e.Message}");
            }
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        static void GenerateAndroid(long[] pattern,int[] amplitudes)
        {
            if (_androidVibrator == null || !_androidVibrator.Call<bool>("hasVibrator")) return;
            try
            {
                if (_androidApiLevel >= 26)
                {
                    var effect = _vibrationEffectClass.CallStatic<AndroidJavaObject>(
                        "createWaveform", 
                        pattern, 
                        amplitudes, 
                        -1); // No repeat
                    _androidVibrator.Call("vibrate", effect);
                }
                else
                {
                    _androidVibrator.Call("vibrate", pattern, -1);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to generate Android haptic : {e.Message}");
            }
        }
#endif

        public static bool IsSupported()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return true; // All iPhones since iPhone 7 support haptics
#elif UNITY_ANDROID && !UNITY_EDITOR
        return _androidVibrator != null && _androidVibrator.Call<bool>("hasVibrator");
#else
            return false;
#endif
        }

        public static bool HapticsEnabled
        {
            get => _hapticsEnabled;
            set => _hapticsEnabled = value;
        }
    }
}