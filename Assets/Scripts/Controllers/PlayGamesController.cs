using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeachHero
{
    public class PlayGamesController : SingleTon<PlayGamesController>
    {
        [Header("Google Play Games Settings")]
        [SerializeField] private string leaderboardID;
        [SerializeField] private bool debugLogEnabled = false;

        [Header("Save Settings")]
        private bool isLoading = false;
        private bool isSaving;
        [SerializeField] private string fileName = "MySaveFile";



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

        #region Save & Load
        public class SaveData
        {
            public string playerName;
            public int score;
        }
        public void SaveDataToJson()
        {
            if (!IsAuthenticated)
            {
                DebugUtils.LogWarning("User is not authenticated to Google Play Services");
                return;
            }

            if (isSaving)
            {
                DebugUtils.LogWarning("Already saving data");
                return;
            }

            isSaving = true;
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseMostRecentlySaved,
                (status, metadata) =>
                {
                    if (status != SavedGameRequestStatus.Success)
                    {
                        DebugUtils.LogError("Error opening saved game");
                        isSaving = false;
                        return;
                    }

                    SaveData data = new SaveData
                    {
                        playerName = "John",
                        score = UnityEngine.Random.Range(0, 101)
                    };

                    string jsonString = JsonUtility.ToJson(data);
                    byte[] savedData = Encoding.ASCII.GetBytes(jsonString);

                    SavedGameMetadataUpdate updatedMetadata = new SavedGameMetadataUpdate.Builder().WithUpdatedDescription("Saved game at " + DateTime.Now).Build();

                    savedGameClient.CommitUpdate(
                        metadata,
                        updatedMetadata,
                        savedData,
                        (commitStatus, _) =>
                        {
                            isSaving = false;
                            bool success = commitStatus == SavedGameRequestStatus.Success;
                            DebugUtils.Log(success ? "Data saved successfully" : "Error saving data");
                        });
                });
        }

        public void LoadDataFromJson()
        {
            if (!IsAuthenticated)
            {
                DebugUtils.LogWarning("User is not authenticated to Google Play Services");
                return;
            }
            if (isLoading)
            {
                DebugUtils.LogWarning("Already loading data");
                return;
            }
            isLoading = true;
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseMostRecentlySaved,
                (status, metadata) =>
                {
                    if (status != SavedGameRequestStatus.Success)
                    {
                        DebugUtils.LogError("Error opening saved game");
                        isLoading = false;
                        return;
                    }
                    savedGameClient.ReadBinaryData(metadata, (readStatus, data) =>
                    {
                        isLoading = false;
                        if (readStatus != SavedGameRequestStatus.Success)
                        {
                            DebugUtils.LogError("Error reading saved game data");
                            return;
                        }
                        string jsonString = Encoding.ASCII.GetString(data);
                        SaveData loadedData = JsonUtility.FromJson<SaveData>(jsonString);
                        DebugUtils.Log($"Loaded Data: Player Name - {loadedData.playerName}, Score - {loadedData.score}");
                    });
                });
        }

        #endregion
    }
}
