using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bokka
{
    public interface ISkinData
    {
        string ID { get; }
        int Hash { get; }
        bool IsUnlocked { get; }
        AbstractSkinsDatabase SkinsProvider { get; }

        void Initialise(AbstractSkinsDatabase provider);
        void Unlock();
        
    }
}