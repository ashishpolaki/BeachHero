using System;
using System.Threading.Tasks;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

namespace BeachHero
{
    public class PlayGamesController : SingleTon<PlayGamesController>
    {
        [Header("Google Play Games Settings")]
        [SerializeField] private string leaderboardID;
        [SerializeField] private bool debugLogEnabled = false;

        #region Properties
        public bool IsAuthenticated => PlayGamesPlatform.Instance != null && PlayGamesPlatform.Instance.IsAuthenticated();
        public string UserId => IsAuthenticated ? PlayGamesPlatform.Instance.GetUserId() : string.Empty;
        public string DisplayName => IsAuthenticated ? PlayGamesPlatform.Instance.GetUserDisplayName() : string.Empty;
        #endregion

        #region Initialization & Authentication
        public void InitializeGPGS()
        {
            PlayGamesPlatform.DebugLogEnabled = debugLogEnabled;
            PlayGamesPlatform.Activate();
            DebugUtils.Log("[PlayGamesController] GPGS Initialized.");
        }

        /// <summary>
        /// Async silent authentication for game startup / auto-login.
        /// </summary>
        public Task<bool> AuthenticateAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            InitializeGPGS();

            if (IsAuthenticated)
            {
                tcs.SetResult(true);
                return tcs.Task;
            }

            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                bool success = (status == SignInStatus.Success);

                if (success)
                {
                    DebugUtils.Log($"[PlayGamesController] Authentication Successful: {DisplayName} ({UserId})");
                }
                else
                {
                    DebugUtils.LogWarning($"[PlayGamesController] Authentication Failed: {status}");
                }

                tcs.SetResult(success);
            });

            return tcs.Task;
        }

        /// <summary>
        /// Manual Sign-in call with callback.
        /// </summary>
        public void SignIn(Action<bool> onComplete = null)
        {
            InitializeGPGS();

            if (IsAuthenticated)
            {
                onComplete?.Invoke(true);
                return;
            }

            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                bool success = (status == SignInStatus.Success);
                DebugUtils.Log(success ? $"[PlayGamesController] Sign-in Successful: {DisplayName} ({UserId})" :
                    $"[PlayGamesController] Sign-in Failed: {status}");
                if (success)
                {
                    SaveSystem.SaveInt(StringUtils.AUTH_LOGIN_TYPE, 1); // 1 = GPGS
                }

                onComplete?.Invoke(success);
            });
        }
        #endregion

        #region Leaderboards
        public void SubmitScore()
        {
            if (!NetworkController.IsInternetAvailable)
            {
                return;
            }

            int totalScore = SaveSystem.LoadInt(StringUtils.TOTAL_SCORE, 0);

            if (string.IsNullOrEmpty(leaderboardID))
            {
                DebugUtils.LogError("[PlayGamesController] Leaderboard ID not set in Inspector");
                return;
            }

            if (!IsAuthenticated)
            {
                DebugUtils.LogWarning("[PlayGamesController] User not authenticated. Attempting sign-in before submitting score.");
                SignIn(success =>
                {
                    if (success) SubmitScore();
                });
                return;
            }

            PlayGamesPlatform.Instance.ReportScore(totalScore, leaderboardID, success =>
            {
                if (success)
                {
                    DebugUtils.Log($"[PlayGamesController] Score {totalScore} submitted successfully.");
                }
                else
                {
                    DebugUtils.LogError("[PlayGamesController] Failed to submit score.");
                }
            });
        }

        public void ShowLeaderboardUI()
        {
            if (!NetworkController.IsInternetAvailable)
            {
                DebugUtils.LogWarning("[PlayGamesController] No internet connection available. Showing NoInternet screen.");
                NetworkController.ShowNoInternetScreen(NetworkActionType.Leaderboard);
                return;
            }

            if (string.IsNullOrEmpty(leaderboardID))
            {
                DebugUtils.LogError("[PlayGamesController] Leaderboard ID not set in Inspector");
                return;
            }

            if (!IsAuthenticated)
            {
                DebugUtils.LogWarning("[PlayGamesController] User not authenticated. Attempting sign-in before showing leaderboard.");
                SignIn(success =>
                {
                    if (success) ShowLeaderboardUI();
                });
                return;
            }

            PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardID);
        }
        #endregion
    }
}
