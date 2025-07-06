using UnityEngine;

namespace BeachHero
{
    public class SaveSystem : MonoBehaviour
    {
        public void Init()
        {
        }
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
    }
}
