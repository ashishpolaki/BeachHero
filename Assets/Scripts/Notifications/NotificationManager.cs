using System;
using UnityEngine;
using BeachHero;

#if UNITY_ANDROID
using Unity.Notifications.Android;
using AndroidImportance = Unity.Notifications.Android.Importance;
#else
using AndroidImportance = System.Int32;
#endif

#if UNITY_IOS
using Unity.Notifications.iOS;
using IOSAuthOptions = Unity.Notifications.iOS.AuthorizationOption;
#else
using IOSAuthOptions = System.Object;
#endif

namespace Shared.PushNotifications
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        [Header("General")]
        [SerializeField] private bool logEvents = true;

        [Header("Android Settings")]
        [SerializeField] private string androidChannelId = "default_channel";
        [SerializeField] private string androidChannelName = "Game Notifications";
        [SerializeField] private string androidChannelDescription = "Game reminders";
        [SerializeField] private AndroidImportance androidImportance = AndroidImportance.Default;
        [SerializeField] private string androidSmallIcon = "icon_small";
        [SerializeField] private string androidLargeIcon = "icon_large";
        [SerializeField] private float notificationDelaySeconds = 10;

        [Header("iOS Settings")]
#if UNITY_IOS
        [SerializeField]
        private IOSAuthOptions iosAuthOptions =
            IOSAuthOptions.Alert | IOSAuthOptions.Sound;
#else
        [SerializeField]
        private IOSAuthOptions iosAuthOptions = null;
#endif

        private ILocalNotificationService notificationService;

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Cancel(androidChannelId);
                Schedule(androidChannelId, androidChannelName, androidChannelDescription, DateTime.UtcNow.AddSeconds(notificationDelaySeconds), string.Empty);
            }
            else
            {
                Cancel(androidChannelId);
            }
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            notificationService = LocalNotificationServiceFactory.Create(
                logEvents,
                androidChannelId,
                androidChannelName,
                androidChannelDescription,
                androidImportance,
                androidSmallIcon,
                androidLargeIcon,
                iosAuthOptions
            );

            if (logEvents)
            {
                DebugUtils.Log("[NotificationManager] Initialized (Local Notifications)");
            }
        }

        #endregion

        #region Public API

        public void Schedule(
            string id,
            string title,
            string message,
            DateTime fireTimeUtc,
            string payload = null)
        {
            if (notificationService == null)
                return;

            var request = new LocalNotificationRequest
            {
                Id = id,
                Title = title,
                Message = message,
                FireTimeUtc = fireTimeUtc,
                Payload = payload
            };

            notificationService.Schedule(request);
        }

        public void Cancel(string id)
        {
            if (notificationService == null || string.IsNullOrEmpty(id))
                return;

            notificationService.Cancel(id);
        }

        public void CancelAll()
        {
            if (notificationService == null)
                return;

            notificationService.CancelAll();
        }

        #endregion
    }
}
