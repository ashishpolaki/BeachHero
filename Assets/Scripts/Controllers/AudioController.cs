using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class AudioController : SingleTon<AudioController>
    {
        [SerializeField] private AudioSettingsSO audioSettings;
        [SerializeField] private int audioSourcesPoolSize = 4;
        [SerializeField] private float fadeMusicDuration = 0.3f;
        private AudioSource gameMusicSource;
        private float gameMusicVolumeMultiplier = 1.0f;
        private List<AudioSourceCase> audioSourcesPool;

        private bool isMusicOn = true;
        private bool isSoundOn = true;

        public void Init()
        {
            foreach (var audioClip in audioSettings.audioClips)
            {
                if (audioClip.audioType == AudioType.GameMusic)
                {
                    CreateMusicSource(audioClip);
                    break;
                }
            }

            // Initialize the audio sources pool
            audioSourcesPool = new List<AudioSourceCase>();
            for (int i = 0; i < audioSourcesPoolSize; i++)
            {
                audioSourcesPool.Add(new AudioSourceCase());
            }

            isMusicOn = SaveSystem.LoadBool(StringUtils.MUSIC_ON, true);
            isSoundOn = SaveSystem.LoadBool(StringUtils.SOUND_ON, true);

            PlayGameMusic();
        }

        private void OnDestroy()
        {
            ReleaseSources();
            if (gameMusicSource != null)
            {
                Destroy(gameMusicSource.gameObject);
            }
        }

        #region Game Music
        public void OnGameMusicToggleChange(bool isOn)
        {
            isMusicOn = isOn;
            SaveSystem.SaveBool(StringUtils.MUSIC_ON, isOn);
            if (isOn)
            {
                PlayGameMusic();
            }
            else
            {
                if (gameMusicSource != null)
                {
                    gameMusicSource.Stop();
                }
            }
        }
        public void OnGameMusicVolumeChange(float volumeChange)
        {
            gameMusicSource.volume = volumeChange * gameMusicVolumeMultiplier;
            SaveSystem.SaveFloat(StringUtils.GAME_MUSIC_VOLUME, volumeChange);
        }

        private void CreateMusicSource(AudioData audioData)
        {
            var gameObject = new GameObject("[MUSIC SOURCE OBJECT]");
            DontDestroyOnLoad(gameObject);
            gameMusicSource = gameObject.AddComponent<AudioSource>();
            gameMusicSource.playOnAwake = false;
            gameMusicSource.clip = audioData.clip;
            gameMusicSource.loop = true;
        }

        public void PlayGameMusic()
        {
            if (!isMusicOn || gameMusicSource == null)
            {
                return;
            }
            if (gameMusicSource != null)
            {
                gameMusicSource.volume = 0.0f;
                gameMusicSource.Stop();
            }
            gameMusicSource.Play();
            FadeMusicVolume();
        }
        private void FadeMusicVolume()
        {
            float volume = SaveSystem.LoadFloat(StringUtils.GAME_MUSIC_VOLUME, audioSettings.GetAudioVolume(AudioType.GameMusic));
            gameMusicSource.volume = volume * gameMusicVolumeMultiplier;
            gameMusicSource.DOFade(volume, fadeMusicDuration);
        }
        #endregion

        #region SFX
        public void OnSoundToggleChange(bool isOn)
        {
            isSoundOn = isOn;
            SaveSystem.SaveBool(StringUtils.SOUND_ON, isOn);
            if (!isOn)
            {
                ReleaseSources();
            }
        }
        public void PlaySound(AudioType audioType)
        {
            if (!isSoundOn)
            {
                return;
            }
            AudioSourceCase sourceCase = GetAudioSource();
            AudioSource source = sourceCase.AudioSource;

            source.spatialBlend = 0.0f; // 2D sound
            source.pitch = audioSettings.GetAudioPitch(audioType);

            // Assuming you have a method to get the appropriate audio clip for the type
            AudioClip clip = audioSettings.GetAudioClip(audioType);
            if (clip != null)
            {
                sourceCase.Play(clip, GetVolume(audioType));
            }
        }
        private void ReleaseSources()
        {
            foreach (var audioSourceCase in audioSourcesPool)
            {
                if (audioSourceCase == null || audioSourceCase.AudioSource == null || audioSourceCase.AudioSource.clip == null) continue;
                if (audioSourceCase.IsPlaying)
                {
                    audioSourceCase.AudioSource.Stop();
                }
                // GameObject.Destroy(audioSourceCase.GameObject);
            }
            //   audioSourcesPool.Clear();
        }
        private AudioSourceCase GetAudioSource()
        {
            foreach (AudioSourceCase audioSource in audioSourcesPool)
            {
                if (!audioSource.IsPlaying)
                {
                    return audioSource;
                }
            }

            AudioSourceCase createdSource = new AudioSourceCase();
            audioSourcesPool.Add(createdSource);
            return createdSource;
        }
        private float GetVolume(AudioType audioType)
        {
            return audioSettings.GetAudioVolume(audioType);
        }
        #endregion
    }

    public class AudioSourceCase
    {
        private AudioSource audioSource;
        public AudioSource AudioSource => audioSource;

        public bool IsPlaying => audioSource.isPlaying;

        private float clipVolume;

        private GameObject gameObject;
        public GameObject GameObject => gameObject;

        public AudioSourceCase()
        {
            gameObject = new GameObject("[AUDIO SOURCE OBJECT]");
            GameObject.DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        public void Play(AudioClip audioClip, float clipVolume)
        {
            audioSource.clip = audioClip;
            audioSource.volume = clipVolume;
            audioSource.loop = false; // Assuming SFX are not looped
            audioSource.Play();
        }

        public void OverrideVolume(AudioType type, float volume)
        {
            // if (!audioSource.isPlaying || audioType != type) return;

            audioSource.volume = volume * clipVolume;
        }
    }
}
