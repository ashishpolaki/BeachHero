// TweenManager.cs - Complete Static Manager for LitMotion

using UnityEngine;
using System.Collections.Generic;
using LitMotion;
using LitMotion.Extensions;

namespace BeachHero.LitTween
{
    using UnityEngine;
    using System.Collections.Generic;
    using LitMotion;
    using LitMotion.Extensions;

    /// <summary>
    /// Static manager class for managing LitMotion tweens with automatic tracking and grouping capabilities.
    /// Zero-allocation, high-performance tween library wrapper.
    /// </summary>
    public static class TweenManager
    {
        #region Motion Tracking Dictionaries

        private static Dictionary<string, List<MotionHandle>> _taggedMotions =
            new Dictionary<string, List<MotionHandle>>();

        private static Dictionary<MotionHandle, string> _motionToTag = new Dictionary<MotionHandle, string>();
        private static List<MotionHandle> _untaggedMotions = new List<MotionHandle>();

        private static Dictionary<object, List<MotionHandle>> _groupedMotions =
            new Dictionary<object, List<MotionHandle>>();

        // Thread safety for multi-threaded access
        private static readonly object _lockObject = new object();

        #endregion

        #region Basic Motion Creation with Tracking

        /// <summary>
        /// Creates a float tween with optional tagging.
        /// </summary>
        public static MotionHandle CreateFloat(float from, float to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration).RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a Vector3 tween with optional tagging.
        /// </summary>
        public static MotionHandle CreateVector3(Vector3 from, Vector3 to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration).RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a Vector2 tween with optional tagging.
        /// </summary>
        public static MotionHandle CreateVector2(Vector2 from, Vector2 to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration).RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a Color tween with optional tagging.
        /// </summary>
        public static MotionHandle CreateColor(Color from, Color to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration).RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a Quaternion tween with optional tagging.
        /// </summary>
        public static MotionHandle CreateQuaternion(Quaternion from, Quaternion to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration).RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates an integer tween with optional tagging.
        /// </summary>
        public static MotionHandle CreateInt(int from, int to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration).RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        #endregion

        #region String Motions (Zero Allocation)

        /// <summary>
        /// Creates a zero-allocation string tween for TextMeshPro with optional tagging.
        /// </summary>
        public static MotionHandle CreateString(string from, string to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.String.Create128Bytes(from, to, duration).RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a rich text string tween with optional tagging.
        /// </summary>
        public static MotionHandle CreateRichText(string from, string to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.String.Create128Bytes(from, to, duration)
                .WithRichText()
                .RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a scrambled text tween with optional tagging.
        /// </summary>
        public static MotionHandle CreateScrambledText(string from, string to, float duration,
            ScrambleMode scrambleMode = ScrambleMode.All, string tag = null, object groupKey = null)
        {
            var handle = LMotion.String.Create128Bytes(from, to, duration)
                .WithScrambleChars(scrambleMode)
                .RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        #endregion

        #region Special Motions (Corrected API - Looking at actual LitMotion source patterns)

        /// <summary>
        /// Creates a punch motion (damped oscillation) with optional tagging.
        /// </summary>
        public static MotionHandle CreatePunch(float from, float to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Punch.Create(from, to, duration).RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a Vector3 punch motion with optional tagging.
        /// </summary>
        public static MotionHandle CreatePunchVector3(Transform target, Vector3 from, Vector3 to, float duration,
            string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Punch.Create(from, to, duration).Bind(x => target.rotation = Quaternion.Euler(x));
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a float shake motion with optional tagging.
        /// </summary>
        public static MotionHandle CreateShake(float strength, float frequency, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Shake.Create(strength, frequency, duration).RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a Vector3 shake motion with optional tagging.
        /// CORRECTED: Based on documentation, Shake.Create takes (strength, frequency, duration)
        /// </summary>
        public static MotionHandle CreateShakeVector3(Vector3 start, Vector3 strength, float duration,
            string tag = null, object groupKey = null)
        {
            var handle = LMotion.Shake.Create(start, strength, duration).RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        #endregion

        #region Transform Bindings

        /// <summary>
        /// Creates and binds a position tween to a Transform with optional tagging.
        /// </summary>
        public static MotionHandle TweenPosition(Transform target, Vector3 to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(target.position, to, duration)
                .BindToPosition(target);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates and binds a local position tween to a Transform with optional tagging.
        /// </summary>
        public static MotionHandle TweenLocalPosition(Transform target, Vector3 to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(target.localPosition, to, duration)
                .BindToLocalPosition(target);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates and binds a rotation tween to a Transform with optional tagging.
        /// </summary>
        public static MotionHandle TweenRotation(Transform target, Quaternion to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(target.rotation, to, duration)
                .BindToRotation(target);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        public static MotionHandle TweenLocalRotateBy(Transform target, Quaternion to, float duration,
            string tag = null, object groupKey = null)
        {
            // Use local rotation for child objects
            var handle = LMotion.Create(target.localRotation, to, duration)
                .BindToLocalRotation(target);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        public static MotionHandle TweenRotateBy(Transform target, Vector3 byValue, float duration,
            string tag = null, object groupKey = null)
        {
            // Store initial rotation in Euler for easier additive animation
            var initialEuler = target.eulerAngles;
            var targetEuler = initialEuler + byValue;

            // Animate the Euler angles
            var handle = LMotion.Create(initialEuler, targetEuler, duration)
                .Bind(x => target.eulerAngles = x);

            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        public static MotionHandle TweenLocalRotateBy(Transform target, Vector3 byValue, float duration,
            string tag = null, object groupKey = null)
        {
            // Use local rotation for child objects
            var initialEuler = target.localEulerAngles;
            var targetEuler = initialEuler + byValue;

            var handle = LMotion.Create(initialEuler, targetEuler, duration)
                .Bind(x => target.localEulerAngles = x);

            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates and binds a scale tween to a Transform with optional tagging.
        /// </summary>
        public static MotionHandle TweenScale(Transform target, Vector3 to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(target.localScale, to, duration)
                .BindToLocalScale(target);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates and binds a position tween on the X axis only.
        /// </summary>
        public static MotionHandle TweenPositionX(Transform target, float to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(target.position.x, to, duration)
                .BindToPositionX(target);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates and binds a shake motion to Transform position.
        /// </summary>
        public static MotionHandle TweenShakePosition(Transform target, Vector3 start, Vector3 strength,
            float duration, string tag = null, object groupKey = null)
        {
            var handle = LMotion.Shake.Create(start, strength, duration)
                .BindToPosition(target);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates and binds a shake motion to Transform rotation.
        /// </summary>
        public static MotionHandle TweenShakeRotation(Transform target, Vector3 start, Vector3 strength, float duration,
            string tag = null, object groupKey = null)
        {
            // First get the current rotation
            var currentRotation = target.rotation;

            // Create a shake motion and bind it to a lambda that updates the rotation
            var handle = LMotion.Shake.Create(start, strength, duration)
                .Bind(shakeOffset =>
                {
                    // Convert shake offset (Vector3) to a rotation offset
                    // This is an approximation - you might need to adjust based on your needs
                    var rotationOffset = Quaternion.Euler(shakeOffset);
                    target.rotation = currentRotation * rotationOffset;
                });

            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        #endregion

        #region UI & TextMeshPro Bindings

        /// <summary>
        /// Creates and binds a color tween to a SpriteRenderer with optional tagging.
        /// </summary>
        public static MotionHandle TweenSpriteColor(SpriteRenderer target, Color to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(target.color, to, duration)
                .BindToColor(target);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates and binds a zero-allocation text tween to TextMeshPro with optional tagging.
        /// </summary>
        public static MotionHandle TweenTextMeshPro(TMPro.TMP_Text target, string to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.String.Create128Bytes(target.text, to, duration)
                .BindToText(target);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates and binds a color tween to TextMeshPro with optional tagging.
        /// </summary>
        public static MotionHandle TweenTextMeshProColor(TMPro.TMP_Text target, Color to, float duration,
            string tag = null, object groupKey = null)
        {
            var handle = LMotion.Create(target.color, to, duration)
                .BindToColor(target);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Animates individual TextMeshPro character colors.
        /// </summary>
        public static MotionHandle TweenTMPCharColor(TMPro.TMP_Text text, int charIndex, Color from, Color to,
            float duration, string tag = null, object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration)
                .BindToTMPCharColor(text, charIndex);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Binds to Debug.unityLogger to display values in Console on update.
        /// </summary>
        public static MotionHandle BindToUnityLogger(float from, float to, float duration, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration)
                .BindToUnityLogger();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        #endregion

        #region Custom Property Bindings

        /// <summary>
        /// Creates a float tween and binds it to a custom property setter.
        /// </summary>
        public static MotionHandle BindToFloat(float from, float to, float duration, System.Action<float> setter,
            string tag = null, object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration)
                .Bind(setter);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a Vector3 tween and binds it to a custom property setter.
        /// </summary>
        public static MotionHandle BindToVector3(Vector3 from, Vector3 to, float duration,
            System.Action<Vector3> setter, string tag = null, object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration)
                .Bind(setter);
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        #endregion

        #region Motion Tracking & Management

        /// <summary>
        /// Tracks a motion handle internally for management.
        /// </summary>
        private static void TrackMotion(MotionHandle handle, string tag, object groupKey)
        {
            lock (_lockObject)
            {
                // Track by tag
                if (!string.IsNullOrEmpty(tag))
                {
                    if (!_taggedMotions.ContainsKey(tag))
                    {
                        _taggedMotions[tag] = new List<MotionHandle>();
                    }

                    _taggedMotions[tag].Add(handle);
                    _motionToTag[handle] = tag;
                }
                else
                {
                    _untaggedMotions.Add(handle);
                }

                // Track by group
                if (groupKey != null)
                {
                    if (!_groupedMotions.ContainsKey(groupKey))
                    {
                        _groupedMotions[groupKey] = new List<MotionHandle>();
                    }

                    _groupedMotions[groupKey].Add(handle);
                }

                // Auto-cleanup when motion completes
                handle.Complete();
                UntrackMotion(handle);
            }
        }

        /// <summary>
        /// Removes a motion from tracking when it completes.
        /// </summary>
        private static void UntrackMotion(MotionHandle handle)
        {
            lock (_lockObject)
            {
                // Remove from tag tracking
                if (_motionToTag.TryGetValue(handle, out string tag))
                {
                    if (_taggedMotions.ContainsKey(tag))
                    {
                        _taggedMotions[tag].Remove(handle);
                        if (_taggedMotions[tag].Count == 0)
                        {
                            _taggedMotions.Remove(tag);
                        }
                    }

                    _motionToTag.Remove(handle);
                }
                else
                {
                    _untaggedMotions.Remove(handle);
                }

                // Remove from group tracking
                var groupsToRemove = new List<object>();
                foreach (var group in _groupedMotions)
                {
                    group.Value.Remove(handle);
                    if (group.Value.Count == 0)
                    {
                        groupsToRemove.Add(group.Key);
                    }
                }

                foreach (var key in groupsToRemove)
                {
                    _groupedMotions.Remove(key);
                }
            }
        }

        /// <summary>
        /// Gets all active motions with a specific tag.
        /// </summary>
        public static List<MotionHandle> GetMotionsByTag(string tag)
        {
            lock (_lockObject)
            {
                if (_taggedMotions.TryGetValue(tag, out var motions))
                {
                    // Filter out completed motions
                    motions.RemoveAll(h => !h.IsActive());
                    return new List<MotionHandle>(motions);
                }

                return new List<MotionHandle>();
            }
        }

        /// <summary>
        /// Gets all active motions in a specific group.
        /// </summary>
        public static List<MotionHandle> GetMotionsByGroup(object groupKey)
        {
            lock (_lockObject)
            {
                if (_groupedMotions.TryGetValue(groupKey, out var motions))
                {
                    // Filter out completed motions
                    motions.RemoveAll(h => !h.IsActive());
                    return new List<MotionHandle>(motions);
                }

                return new List<MotionHandle>();
            }
        }

        /// <summary>
        /// Gets all active motions.
        /// </summary>
        public static List<MotionHandle> GetAllActiveMotions()
        {
            lock (_lockObject)
            {
                var allMotions = new List<MotionHandle>();

                // Add tagged motions
                foreach (var motions in _taggedMotions.Values)
                {
                    allMotions.AddRange(motions.FindAll(h => h.IsActive()));
                }

                // Add untagged motions
                allMotions.AddRange(_untaggedMotions.FindAll(h => h.IsActive()));

                return allMotions;
            }
        }

        /// <summary>
        /// Counts active motions by tag.
        /// </summary>
        public static int CountMotionsByTag(string tag)
        {
            lock (_lockObject)
            {
                if (_taggedMotions.TryGetValue(tag, out var motions))
                {
                    return motions.FindAll(h => h.IsActive()).Count;
                }

                return 0;
            }
        }

        /// <summary>
        /// Counts all active motions.
        /// </summary>
        public static int CountAllActiveMotions()
        {
            lock (_lockObject)
            {
                int count = 0;

                foreach (var motions in _taggedMotions.Values)
                {
                    count += motions.FindAll(h => h.IsActive()).Count;
                }

                count += _untaggedMotions.FindAll(h => h.IsActive()).Count;

                return count;
            }
        }

        #endregion

        #region Motion Control & Management

        /// <summary>
        /// Completes all motions with a specific tag immediately.
        /// </summary>
        public static void CompleteMotionsByTag(string tag)
        {
            lock (_lockObject)
            {
                var motions = GetMotionsByTag(tag);
                foreach (var handle in motions)
                {
                    if (handle.IsActive())
                    {
                        handle.Complete();
                    }
                }
            }
        }

        /// <summary>
        /// Cancels all motions with a specific tag.
        /// </summary>
        public static void CancelMotionsByTag(string tag)
        {
            lock (_lockObject)
            {
                var motions = GetMotionsByTag(tag);
                foreach (var handle in motions)
                {
                    if (handle.IsActive())
                    {
                        handle.Cancel();
                    }
                }
            }
        }

        /// <summary>
        /// Completes all active motions.
        /// </summary>
        public static void CompleteAllMotions()
        {
            lock (_lockObject)
            {
                var allMotions = GetAllActiveMotions();
                foreach (var handle in allMotions)
                {
                    handle.Complete();
                }
            }
        }

        /// <summary>
        /// Cancels all active motions.
        /// </summary>
        public static void CancelAllMotions()
        {
            lock (_lockObject)
            {
                var allMotions = GetAllActiveMotions();
                foreach (var handle in allMotions)
                {
                    handle.Cancel();
                }
            }
        }

        /// <summary>
        /// Checks if a motion handle is currently active.
        /// </summary>
        public static bool IsMotionActive(MotionHandle handle)
        {
            return handle.IsActive();
        }

        /// <summary>
        /// Completes a motion immediately (jumps to end state).
        /// </summary>
        public static void CompleteMotion(MotionHandle handle)
        {
            if (handle.IsActive())
            {
                handle.Complete();
            }
        }

        /// <summary>
        /// Cancels a motion.
        /// </summary>
        public static void CancelMotion(MotionHandle handle)
        {
            if (handle.IsActive())
            {
                handle.Cancel();
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Creates a delayed callback (no tween, just a timer) with optional tagging.
        /// </summary>
        public static MotionHandle CreateDelay(float delay, System.Action callback, string tag = null,
            object groupKey = null)
        {
            var handle = LMotion.Create(0f, 0f, delay)
                .WithOnComplete(callback)
                .RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a float tween with specific easing and optional tagging.
        /// </summary>
        public static MotionHandle CreateFloatWithEase(float from, float to, float duration, Ease ease,
            string tag = null, object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration)
                .WithEase(ease)
                .RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a looping float tween with optional tagging.
        /// </summary>
        public static MotionHandle CreateFloatLoop(float from, float to, float duration, int loops,
            LoopType loopType = LoopType.Restart, string tag = null, object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration)
                .WithLoops(loops, loopType)
                .RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a float tween with a start delay and optional tagging.
        /// </summary>
        public static MotionHandle CreateFloatDelayed(float from, float to, float duration, float delay,
            string tag = null, object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration)
                .WithDelay(delay)
                .RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a float tween with a completion callback.
        /// </summary>
        public static MotionHandle CreateFloatWithCallback(float from, float to, float duration, System.Action callback,
            string tag = null, object groupKey = null)
        {
            var handle = LMotion.Create(from, to, duration)
                .WithOnComplete(callback)
                .RunWithoutBinding();
            TrackMotion(handle, tag, groupKey);
            return handle;
        }

        /// <summary>
        /// Creates a sequence to combine multiple motions.
        /// </summary>
        public static MotionSequenceBuilder CreateSequence()
        {
            return LSequence.Create();
        }

        #endregion

        #region Cleanup & Debugging

        /// <summary>
        /// Cleans up completed motions from tracking (called automatically, but can be manually triggered).
        /// </summary>
        public static void CleanupCompletedMotions()
        {
            lock (_lockObject)
            {
                // Clean up tagged motions
                var tagsToRemove = new List<string>();
                foreach (var kvp in _taggedMotions)
                {
                    kvp.Value.RemoveAll(h => !h.IsActive());
                    if (kvp.Value.Count == 0)
                    {
                        tagsToRemove.Add(kvp.Key);
                    }
                }

                foreach (var tag in tagsToRemove)
                {
                    _taggedMotions.Remove(tag);
                }

                // Clean up motion-to-tag mapping
                var handlesToRemove = new List<MotionHandle>();
                foreach (var kvp in _motionToTag)
                {
                    if (!kvp.Key.IsActive())
                    {
                        handlesToRemove.Add(kvp.Key);
                    }
                }

                foreach (var handle in handlesToRemove)
                {
                    _motionToTag.Remove(handle);
                }

                // Clean up untagged motions
                _untaggedMotions.RemoveAll(h => !h.IsActive());

                // Clean up grouped motions
                var groupsToRemove = new List<object>();
                foreach (var kvp in _groupedMotions)
                {
                    kvp.Value.RemoveAll(h => !h.IsActive());
                    if (kvp.Value.Count == 0)
                    {
                        groupsToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in groupsToRemove)
                {
                    _groupedMotions.Remove(key);
                }
            }
        }

        /// <summary>
        /// Logs debug information about currently tracked motions.
        /// </summary>
        public static void LogDebugInfo()
        {
            lock (_lockObject)
            {
                int totalActive = CountAllActiveMotions();
                Debug.Log($"=== TweenManager Debug Info ===");
                Debug.Log($"Total Active Motions: {totalActive}");
                Debug.Log($"Tagged Motions Count: {_taggedMotions.Count} different tags");
                Debug.Log($"Untagged Motions: {_untaggedMotions.Count}");
                Debug.Log($"Grouped Motions: {_groupedMotions.Count} different groups");

                foreach (var kvp in _taggedMotions)
                {
                    int activeCount = kvp.Value.FindAll(h => h.IsActive()).Count;
                    Debug.Log($"  Tag '{kvp.Key}': {activeCount} active, {kvp.Value.Count} total");
                }
            }
        }

        #endregion
    }
}