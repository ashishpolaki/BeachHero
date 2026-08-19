using UnityEngine;

namespace BeachHero
{
    public class SaveSystem : MonoBehaviour
    {
        public static GameData CurrentData { get; private set; }

        public static void Init()
        {
            ES3.Init();
            LoadGameData();
        }

        #region GameData Management
        public static void DeleteGameData()
        {
            // 1. Delete the specific GameData key from ES3
            if (ES3.KeyExists(StringUtils.GAME_DATA))
            {
                ES3.DeleteKey(StringUtils.GAME_DATA);
            }
            // 2. Clear the in-memory cached instance
            CurrentData = null;
            DebugUtils.Log("[SaveSystem] GameData cleared successfully.");
        }
        public static GameData LoadGameData()
        {
            if (CurrentData != null)
            {
                return CurrentData;
            }

            if (ES3.KeyExists(StringUtils.GAME_DATA))
            {
                string json = ES3.Load<string>(StringUtils.GAME_DATA);
                CurrentData = GameData.FromJson(json);
            }
            else
            {
                CurrentData = GameData.CreateDefault();
                SaveGameData();
            }

            return CurrentData;
        }

        public static void SaveGameData(GameData data = null)
        {
            if (data != null)
            {
                CurrentData = data;
            }

            if (CurrentData == null)
            {
                CurrentData = GameData.CreateDefault();
            }

            string json = CurrentData.ToJson();
            ES3.Save(StringUtils.GAME_DATA, json);
        }

        public static void MergeAndSaveWithCloudData(GameData cloudData)
        {
            if (CurrentData == null)
            {
                LoadGameData();
            }

            if (cloudData != null)
            {
                CurrentData.MergeWithCloudData(cloudData);
            }

            SaveGameData();
        }
        #endregion

        #region Legacy Primitive Helpers
        public static void SaveInt(string _saveString, int _value)
        {
            ES3.Save(_saveString, _value);
        }
        public static int LoadInt(string _saveString, int _defaultValue)
        {
            return ES3.Load(_saveString, _defaultValue);
        }
        public static bool LoadBool(string _saveString, bool _defaultValue)
        {
            return ES3.Load(_saveString, _defaultValue);
        }
        public static void SaveBool(string _saveString, bool _value)
        {
            ES3.Save(_saveString, _value);
        }
        public static void SaveFloat(string _saveString, float _value)
        {
            ES3.Save(_saveString, _value);
        }
        public static float LoadFloat(string _saveString, float _defaultValue)
        {
            return ES3.Load(_saveString, _defaultValue);
        }
        #endregion
    }
}
