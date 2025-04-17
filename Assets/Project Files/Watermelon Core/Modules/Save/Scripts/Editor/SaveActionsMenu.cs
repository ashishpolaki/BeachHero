using UnityEngine;
using Bokka;
using UnityEditor;

namespace Bokka
{
    public static class SaveActionsMenu
    {
        [MenuItem("Actions/Remove Save", priority = 1)]
        private static void RemoveSave()
        {
            PlayerPrefs.DeleteAll();
            SaveController.DeleteSaveFile();

            Debug.Log("Save files are removed!");
        }

        [MenuItem("Actions/Remove Save", true)]
        private static bool RemoveSaveValidation()
        {
            return !Application.isPlaying;
        }
    }
}