using UnityEngine;

namespace BeachHero
{
    public enum AudioType
    {
        GameMusic,
        Button,
        CharacterPickup,
        Gamelose,
        Gamewin,
        Coin,
    }
    public enum AudioVolumeType
    {
        Music,
        SFX
    }
    [System.Serializable]
    public struct AudioData
    {
        public AudioType audioType;
        public AudioClip clip;
        public float volume;
        public float pitch;
    }
    [CreateAssetMenu(fileName = "AudioSettingsSO", menuName = "Scriptable Objects/Audio/AudioSettingsSO")]
    public class AudioSettingsSO : ScriptableObject
    {
        public AudioData[] audioClips;

        public AudioClip GetAudioClip(AudioType audioType)
        {
            foreach (var audioData in audioClips)
            {
                if (audioData.audioType == audioType)
                {
                    return audioData.clip;
                }
            }
            return null; // Return null if no clip found for the specified type
        }
        public float GetAudioVolume(AudioType audioType)
        {
            foreach (var audioData in audioClips)
            {
                if (audioData.audioType == audioType)
                {
                    return audioData.volume;
                }
            }
            return 1.0f; // Default volume if not found
        }
        public float GetAudioPitch(AudioType audioType)
        {
            foreach (var audioData in audioClips)
            {
                if (audioData.audioType == audioType)
                {
                    return audioData.pitch;
                }
            }
            return 1.0f; // Default pitch if not found
        }
    }

}
