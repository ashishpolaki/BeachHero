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

        private const string fileName = "BeachHeroSaveFile";
        private const float AUTH_TIMEOUT = 5f;
        private const float SIGNIN_TIMEOUT = 10f;
        private const float LOAD_TIMEOUT = 5f;

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
        /// Authenticates silently AND automatically loads + merges cloud save data before returning.
        /// </summary>
        public async Task<bool> AutoAuthenticateAsync()
        {
            InitializeGPGS();

            bool isAuthDone = false;
            bool isAuthSuccess = false;

            if (IsAuthenticated)
            {
                isAuthSuccess = true;
                isAuthDone = true;
            }
            else
            {
                PlayGamesPlatform.Instance.Authenticate(status =>
                {
                    isAuthSuccess = (status == SignInStatus.Success);
                    if (isAuthSuccess)
                    {
                        DebugUtils.Log($"[PlayGamesController] Auto-Auth Successful: {DisplayName} ({UserId})");
                    }
                    else
                    {
                        DebugUtils.LogWarning($"[PlayGamesController] Silent Authentication Failed/Not Signed In: {status}");
                    }
                    isAuthDone = true;
                });
            }

            // Timeout for authentication (Max 5 Seconds)
            float authTimer = 0f;
            while (!isAuthDone && authTimer < AUTH_TIMEOUT)
            {
                await Task.Yield();
                authTimer += Time.unscaledDeltaTime;
            }

            if (!isAuthDone)
            {
                DebugUtils.LogWarning("[PlayGamesController] Auto-Auth timed out after 5 seconds.");
                isAuthSuccess = false;
                return isAuthSuccess;
            }

            if (isAuthSuccess)
            {
                // Load and merge cloud data (has its own 5s timeout internally)
                await LoadDataFromCloud();
            }

            return isAuthSuccess;
        }

        public async void SignInASync(Action<bool> onComplete = null)
        {
            InitializeGPGS();
            bool isAuthSuccess = false;
            bool isAuthDone = false;

            if (IsAuthenticated)
            {
                isAuthSuccess = true;
                isAuthDone = true;
            }
            else
            {
                PlayGamesPlatform.Instance.ManuallyAuthenticate(status =>
                {
                    isAuthSuccess = (status == SignInStatus.Success);
                    if (isAuthSuccess)
                    {
                        DebugUtils.Log($"[PlayGamesController] Sign-in Successful: {DisplayName} ({UserId})");
                        SaveSystem.SaveInt(StringUtils.AUTH_LOGIN_TYPE, 1); // 1 = GPGS
                    }
                    else
                    {
                        DebugUtils.LogWarning($"[PlayGamesController] Sign-in Failed: {status}");
                    }
                    isAuthDone = true;
                });
            }

            // Timeout for interactive sign-in (Max 10 Seconds)
            float signinTimer = 0f;

            while (!isAuthDone && signinTimer < SIGNIN_TIMEOUT)
            {
                await Task.Yield();
                signinTimer += Time.unscaledDeltaTime;
            }

            if (!isAuthDone)
            {
                DebugUtils.LogWarning("[PlayGamesController] Sign-in timed out.");
                isAuthSuccess = false;
                onComplete?.Invoke(isAuthSuccess);
                return;
            }

            if (isAuthSuccess)
            {
                await LoadDataFromCloud();
            }

            onComplete?.Invoke(isAuthSuccess);
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
                return;
            }

            PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardID);
        }
        #endregion

        #region Save & Load
        public void SaveDataInCloud()
        {
            if (!NetworkController.IsInternetAvailable)
            {
                DebugUtils.LogWarning("No internet connection available. Cannot save data to cloud.");
                return;
            }

            if (!IsAuthenticated)
            {
                DebugUtils.LogWarning("User is not authenticated to Google Play Services");
                return;
            }

            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseMostRecentlySaved,
                (status, metadata) =>
                {
                    if (status != SavedGameRequestStatus.Success)
                    {
                        DebugUtils.LogError("Error opening saved game");
                        return;
                    }

                    // Get the current game data or create a default one if it doesn't exist
                    GameData data = SaveSystem.CurrentData ?? GameData.CreateDefault();
                    string jsonString = data.ToJson();
                    byte[] savedData = Encoding.UTF8.GetBytes(jsonString);

                    // Update the metadata with a new description (optional)
                    SavedGameMetadataUpdate updatedMetadata = new SavedGameMetadataUpdate.Builder().WithUpdatedDescription("Saved game at " + DateTime.UtcNow).Build();
                    savedGameClient.CommitUpdate(metadata, updatedMetadata, savedData, (commitStatus, _) =>
                        {
                            bool success = commitStatus == SavedGameRequestStatus.Success;
                            DebugUtils.Log(success ? "Data saved successfully to cloud" : "Error saving data to cloud");
                        });
                });
        }

        public async Task<GameData> LoadDataFromCloud()
        {
            if(!NetworkController.IsInternetAvailable)
            {
                DebugUtils.LogWarning("[PlayGamesController] No internet connection available. Cannot load cloud data.");
                return SaveSystem.CurrentData;
            }

            if (!IsAuthenticated)
            {
                DebugUtils.LogWarning("[PlayGamesController] User not authenticated. Cannot load cloud data.");
                return SaveSystem.CurrentData;
            }

            bool isLoadDone = false;

            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseMostRecentlySaved,
                (status, metadata) =>
                {
                    if (status != SavedGameRequestStatus.Success)
                    {
                        DebugUtils.LogError("[PlayGamesController] Error opening saved game: " + status);
                        isLoadDone = true;
                        return;
                    }

                    savedGameClient.ReadBinaryData(metadata, (readStatus, data) =>
                    {
                        // Return if the read operation failed
                        if (readStatus != SavedGameRequestStatus.Success)
                        {
                            DebugUtils.LogError("[PlayGamesController] Error reading saved game data: " + readStatus);
                            isLoadDone = true;
                            return;
                        }

                        if (data != null && data.Length > 0)
                        {
                            string jsonString = Encoding.UTF8.GetString(data);
                            GameData cloudData = GameData.FromJson(jsonString);

                            if (cloudData != null)
                            {
                                // Smart Merge: Merge cloud progress with offline local progress
                                SaveSystem.MergeAndSaveWithCloudData(cloudData);

                                // Immediately update the cloud with the merged result
                                SaveDataInCloud();
                                DebugUtils.Log("[PlayGamesController] Cloud data loaded and merged: " + jsonString);
                            }
                        }

                        isLoadDone = true;
                    });
                });

            // Wait for cloud load to finish (Max 5 Seconds Timeout)
            float loadTimer = 0f;

            while (!isLoadDone && loadTimer < LOAD_TIMEOUT)
            {
                await Task.Yield();
                loadTimer += Time.unscaledDeltaTime;
            }
            if (!isLoadDone)
            {
                DebugUtils.LogWarning("[PlayGamesController] LoadDataFromCloud timed out after 5s. Proceeding with local data.");
            }
            return SaveSystem.CurrentData;
        }
        #endregion
    }
}
