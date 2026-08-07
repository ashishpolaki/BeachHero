using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

namespace BeachHero
{
    public class LeaderboardController : SingleTon<LeaderboardController>
    {
        [Header("Google Play Leaderboard")]
        [SerializeField] private string leaderboardID;
        [SerializeField] private bool debugLogEnabled = false;

        private bool isInitialized;
        private bool isAuthenticating;
        private bool isConnected;

        #region Initialization & Login
        public void InitializeGPGS()
        {
            if (isInitialized)
                return;
            PlayGamesPlatform.DebugLogEnabled = debugLogEnabled;
            PlayGamesPlatform.Activate();
            isInitialized = true;

            SignIn();
        }

        public void SignIn()
        {
            if (PlayGamesPlatform.Instance.IsAuthenticated() || isAuthenticating)
                return;

            isAuthenticating = true;

            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                isAuthenticating = false;
                isConnected = (status == SignInStatus.Success);
            });
        }

        public bool IsAuthenticated()
        {
            return isConnected;
        }
        #endregion

        #region Leaderboard
        public void SubmitScore()
        {
            int totalScore = SaveSystem.LoadInt(StringUtils.TOTAL_SCORE, 0);

            if (string.IsNullOrEmpty(leaderboardID))
            {
                DebugUtils.LogError("Leaderboard ID not set");
                return;
            }

            if (!IsAuthenticated())
            {
                DebugUtils.LogWarning("User not authenticated. Retrying sign-in.");
                SignIn();
                return;
            }

            PlayGamesPlatform.Instance.ReportScore(totalScore, leaderboardID, success =>
            {
                if (success)
                {
                    DebugUtils.Log($"Score {totalScore} submitted successfully");
                }
                else
                {
                    DebugUtils.LogError("Failed to submit score");
                }
            });
        }

        public void ShowLeaderboardUI()
        {
            if (string.IsNullOrEmpty(leaderboardID))
            {
                DebugUtils.LogError("Leaderboard ID not set");
                return;
            }

            if (!IsAuthenticated())
            {
                DebugUtils.LogWarning("User not authenticated. Retrying sign-in.");
                SignIn();
                return;
            }

            PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardID);
        }
        #endregion
    }
}
