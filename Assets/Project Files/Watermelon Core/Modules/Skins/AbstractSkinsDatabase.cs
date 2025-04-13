using System;
using UnityEngine;

namespace Watermelon
{
    public abstract class AbstractSkinsDatabase : ScriptableObject
    {
        public abstract int SkinsCount { get; }
        public abstract Type SkinType { get; }

        public abstract ISkinData GetSkinData(int index);
        public abstract ISkinData GetSkinData(string id);

        public abstract void Init();
    }
}
