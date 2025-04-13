#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    public class ProjectInitSettings : ScriptableObject
    {
        [SerializeField] InitModule[] modules;
        public InitModule[] Modules => modules;

        public void Init(Initializer initializer)
        {
            for (int i = 0; i < modules.Length; i++)
            {
                if(modules[i] != null)
                {
                    modules[i].CreateComponent();
                }
            }
        }
    }
}