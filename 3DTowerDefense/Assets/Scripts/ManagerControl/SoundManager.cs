using System.Collections.Generic;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class SoundManager : Singleton<SoundManager>
    {
        public const string BGM = "BGM";
        public const string ForestBGM = "ForestBGM";
        public const string ButtonSound = "ButtonSound";
        public const string BuildPointSound = "BuildPointSound";
        public const string SellSound1 = "SellSound1";
        public const string SellSound2 = "SellSound2";
        public const string SellSound3 = "SellSound3";

        private bool _musicOn;

        private Dictionary<string, AudioClip> _musicDictionary;
        private Dictionary<string, AudioClip> _effectDictionary;

        [SerializeField] private AudioSource musicSource, effectsSource;

        [SerializeField] private AudioClip[] musicSounds;
        [SerializeField] private AudioClip[] effectSounds;

        private void Awake()
        {
            _effectDictionary = new Dictionary<string, AudioClip>();
            foreach (var s in effectSounds)
            {
                var effectName = s.name;
                _effectDictionary.Add(effectName, s);
            }

            _musicDictionary = new Dictionary<string, AudioClip>();
            foreach (var s in musicSounds)
            {
                var musicName = s.name;
                _musicDictionary.Add(musicName, s);
            }
        }

        public void PlayBGM(string clipName)
        {
            musicSource.Stop();
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