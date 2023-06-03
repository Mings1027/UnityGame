using System;
using System.Collections.Generic;
using GameControl;
using UnityEngine;

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

        private bool _musicOn;
        private Dictionary<string, AudioClip> _effectDictionary;

        [SerializeField] private AudioSource musicSource, effectsSource;
        [SerializeField] private EffectSound[] effectSounds;

        private void Awake()
        {
            _effectDictionary = new Dictionary<string, AudioClip>();
            foreach (var t in effectSounds)
            {
                _effectDictionary.Add(t.effectName, t.effectSource);
            }
        }

        public void PlayBGM()
        {
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