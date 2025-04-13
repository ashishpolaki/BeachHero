using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Audio Clips", menuName = "Data/Core/Audio Clips")]
    public class AudioClips : ScriptableObject
    {
        [BoxGroup("UI", "UI")]
        public AudioClip buttonSound;

        [BoxGroup("Gameplay", "Gameplay")]
        public AudioClip saved;
        [BoxGroup("Gameplay")]
        public AudioClip complete;
        [BoxGroup("Gameplay")]
        public AudioClip failed;
        [BoxGroup("Gameplay")]
        public AudioClip drown;
        [BoxGroup("Gameplay")]
        public AudioClip coin;
    }
}

// -----------------
// Audio Controller v 0.4
// -----------------