using UnityEngine;
using BeachHero;

#if UNITY_ANDROID
using Unity.Notifications.Android;
using Unity.Android;
using AndroidImportance = Unity.Notifications.Android.Importance;
using System;
using UnityEngine.Android;

#else
using AndroidImportance = System.Int32;
#endif

#if UNITY_IOS
using System;
using Unity.Notifications.iOS;
using IOSAuthOptions = Unity.Notifications.iOS.AuthorizationOption;
#else
using IOSAuthOptions = System.Object;
#endif

namespace Shared.PushNotifications
{
    public interface ILocalNotificationService
    {
        void Schedule(LocalNotificationRequest request);
        void Cancel(string platformId);
        void CancelAll();
    }

    public static class LocalNotificationServiceFactory
    {
        public static ILocalNotificationService Create(
            bool logEvents,
            string androidChannelId,
            string androidChannelName,
            string androidChannelDescription,
            AndroidImportance androidImportance,
            string androidSmallIcon,
            string androidLargeIcon,
            IOSAuthOptions iosOptions)
        {
#if UNITY_ANDROID
            return new AndroidLocalNotificationService(logEvents, androidChannelId, androidChannelName, androidChannelDescription, androidImportance, androidSmallIcon, androidLargeIcon);
#elif UNITY_IOS
            return new IOSLocalNotificationService(logEvents, iosOptions);
#else
            return new NoOpLocalNotificationService();
#endif
        }
    }

#if UNITY_ANDROID
    internal class AndroidLocalNotificationService : ILocalNotificationService
    {
        private readonly bool log;
        private readonly string channelId;
        private readonly string channelName;
        private readonly string channelDescription;
        private readonly AndroidImportance importance;
        private readonly string smallIcon;
        private readonly string largeIcon;
        private bool channelRegistered;

        public AndroidLocalNotificationService(bool logEvents, string id, string name, string description, AndroidImportance importance, string smallIcon, string largeIcon)
        {
            log = logEvents;
            channelId = id;
            channelName = name;
            channelDescription = description;
            this.importance = importance;
            this.smallIcon = smallIcon;
            this.largeIcon = largeIcon;
            RegisterChannel();
        }

        private void RegisterChannel()
        {
            if (channelRegistered)
                return;

#if UNITY_ANDROID
            // Request permission on Android 13 (API level 33) and above
            if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            }
#endif

            var channel = new AndroidNotificationChannel
            {
                Id = channelId,
                Name = channelName,
                Description = channelDescription,
                Importance = importance
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
            channelRegistered = true;
            if (log)
            {
                DebugUtils.Log($"[Push] Android channel '{channelId}' registered.");
            }
        }

        public void Schedule(LocalNotificationRequest request)
        {
            RegisterChannel();
            var notification = new AndroidNotification
            {
                Title = request.Title,
                Text = request.Message,
                FireTime = request.FireTimeUtc,
                SmallIcon = smallIcon,
                LargeIcon = largeIcon,
                IntentData = request.Payload ?? string.Empty
            };
            int id = AndroidNotificationCenter.SendNotification(notification, channelId);
            if (log)
            {
                DebugUtils.Log($"[Push] Scheduled Android notification '{request.Title}' ({id}) for {request.FireTimeUtc}.");
            }
          //  return id.ToString();
        }

        public void Cancel(string platformId)
        {
            if (int.TryParse(platformId, out int id))
            {
                AndroidNotificationCenter.CancelNotification(id);
            }
        }

        public void CancelAll()
        {
            AndroidNotificationCenter.CancelAllDisplayedNotifications();
            AndroidNotificationCenter.CancelAllScheduledNotifications();
        }
    }
#endif

#if UNITY_IOS
    internal class IOSLocalNotificationService : ILocalNotificationService
    {
        private readonly bool log;
        private readonly AuthorizationOption options;
        private bool requestedAuth;
        private bool? authorizationGranted;

        public IOSLocalNotificationService(bool logEvents, AuthorizationOption authOptions)
        {
            log = logEvents;
            options = authOptions;
            RequestAuth();
        }

        private void RequestAuth()
        {
            if (requestedAuth) return;
            requestedAuth = true;
            // Fire and forget auth request
            var _ = RequestAuthorizationAsync();
        }

        private async System.Threading.Tasks.Task RequestAuthorizationAsync()
        {
            using (var request = new AuthorizationRequest(options, true))
            {
                while (!request.IsFinished)
                {
                    await System.Threading.Tasks.Task.Yield();
                }
                authorizationGranted = request.Granted;
                if (log)
                {
                    Debug.Log($"[Push] iOS authorization granted: {request.Granted}");
                }
            }
        }
        public string Schedule(LocalNotificationRequest request)
        {
            RequestAuth();

            // If authorization was denied, skip scheduling and surface a log.
            if (authorizationGranted.HasValue && !authorizationGranted.Value)
            {
                if (log)
                {
                    ErrorToastUI.Instance.ShowMessage("iOS notification authorization denied. Skipping schedule.");
                    ("[Push] iOS notification authorization denied. Skipping schedule.").LogWarning();
                }
                return string.Empty;
            }

            var seconds = Math.Max(0, (request.FireTimeUtc - DateTime.UtcNow).TotalSeconds);
            var trigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = TimeSpan.FromSeconds(seconds),
                Repeats = false
            };

            var notification = new iOSNotification
            {
                Identifier = string.IsNullOrEmpty(request.Id) ? Guid.NewGuid().ToString() : request.Id,
                Title = request.Title,
                Body = request.Message,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
                Data = request.Payload ?? string.Empty,
                Trigger = trigger
            };

            iOSNotificationCenter.ScheduleNotification(notification);

            if (log)
            {
                Debug.Log($"[Push] Scheduled iOS notification '{request.Title}' ({notification.Identifier}) for {request.FireTimeUtc}.");
            }

            return notification.Identifier;
        }

        public void Cancel(string platformId)
        {
            if (string.IsNullOrEmpty(platformId)) return;
            iOSNotificationCenter.RemoveScheduledNotification(platformId);
            iOSNotificationCenter.RemoveDeliveredNotification(platformId);
        }

        public void CancelAll()
        {
            iOSNotificationCenter.RemoveAllScheduledNotifications();
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
        }

        public bool? IsAuthorized() => authorizationGranted;
    }
#endif

    internal class NoOpLocalNotificationService : ILocalNotificationService
    {
        public void Schedule(LocalNotificationRequest request) { }
        public void Cancel(string platformId) { }
        public void CancelAll() { }
    }
}
[Serializable]
public struct LocalNotificationRequest
{
    public string Id; // unique per logical reminder
    public string Title;
    public string Message;
    public DateTime FireTimeUtc;
    public string Payload;
    public bool RequireBackground;
}