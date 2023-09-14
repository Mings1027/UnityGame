using System;
using System.Collections.Generic;
using GameControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class SoundManager : Singleton<SoundManager>
    {
        [Serializable]
        public class EffectSound
        {
            public string effectName;
            public AudioClip effectSource;
        }

        [Serializable]
        public class MusicSound
        {
            public string musicName;
            public AudioClip musicClip;
        }

        private bool _musicOn;
        private Dictionary<string, AudioClip> _musicDictionary;
        private Dictionary<string, AudioClip> _effectDictionary;

        [SerializeField] private AudioSource musicSource, effectsSource;
        [SerializeField] private MusicSound[] musicSounds;
        [SerializeField] private EffectSound[] effectSounds;

        private void Awake()
        {
            _musicDictionary = new Dictionary<string, AudioClip>();
            for (var i = 0; i < musicSounds.Length; i++)
            {
                _musicDictionary.Add(musicSounds[i].musicName, musicSounds[i].musicClip);
            }

            _effectDictionary = new Dictionary<string, AudioClip>();
            for (var i = 0; i < effectSounds.Length; i++)
            {
                _effectDictionary.Add(effectSounds[i].effectName, effectSounds[i].effectSource);
            }
        }

        public void PlayBGM(string clipName)
        {
            musicSource.clip = _musicDictionary[clipName];
            musicSource.Play();
            _musicOn = true;
        }

        public void PlaySound(string clipName)
        {
            var clip = _effectDictionary[clipName];
            effectsSource.PlayOneShot(clip);
        }

        public bool BGMToggle()
        {
            if (_musicOn)
            {
                _musicOn = false;
                musicSource.mute = true;
            }
            else
            {
                _musicOn = true;
                musicSource.mute = false;
            }

            return _musicOn;
        }
    }
}