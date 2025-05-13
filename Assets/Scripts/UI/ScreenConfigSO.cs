using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "ScreenConfigSO", menuName = "Scriptable Objects/ScreenConfigSO")]
    public class ScreenConfigSO : ScriptableObject
    {
        public List<BaseScreen> screens;
    }
}
