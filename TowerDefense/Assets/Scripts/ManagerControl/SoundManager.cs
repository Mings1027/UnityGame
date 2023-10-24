using System;
using System.Collections.Generic;
using CustomEnumControl;
using GameControl;
using UnityEngine;
using UnityEngine.Audio;

namespace ManagerControl
{
    public class SoundManager : Singleton<SoundManager>
    {
        [Serializable]
        public class EffectSound
        {
            public SoundEnum effectName;
            public AudioClip effectSource;
        }

        [Serializable]
        public class MusicSound
        {
            public SoundEnum musicName;
            public AudioClip musicClip;
        }

        private bool _bgmOn, _sfxOn;
        private Dictionary<SoundEnum, AudioClip> _musicDictionary;
        private Dictionary<SoundEnum, AudioClip> _effectDictionary;

        private AudioSource musicSource, effectSource;

        [SerializeField] private MusicSound[] musicSounds;
        [SerializeField] private EffectSound[] effectSounds;

        [SerializeField] private AudioMixer audioMixer;

        protected override void Awake()
        {
            base.Awake();
            musicSource = transform.Find("Music Source").GetComponent<AudioSource>();
            effectSource = transform.Find("Effect Source").GetComponent<AudioSource>();

            _musicDictionary = new Dictionary<SoundEnum, AudioClip>();
            for (var i = 0; i < musicSounds.Length; i++)
            {
                _musicDictionary.Add(musicSounds[i].musicName, musicSounds[i].musicClip);
            }

            _effectDictionary = new Dictionary<SoundEnum, AudioClip>();
            for (var i = 0; i < effectSounds.Length; i++)
            {
                _effectDictionary.Add(effectSounds[i].effectName, effectSounds[i].effectSource);
            }
        }

        protected override void Start()
        {
            base.Start();
            _bgmOn = _sfxOn = true;
        }

        public void PlayBGM(SoundEnum clipName)
        {
            musicSource.clip = _musicDictionary[clipName];
            musicSource.Play();
        }

        public void PlaySound(SoundEnum clipName)
        {
            effectSource.clip = _effectDictionary[clipName];
            effectSource.Play();
        }

        public void ToggleBGM(bool active)
        {
            _bgmOn = active;
            audioMixer.SetFloat("BGM", _bgmOn ? 0 : -80);
        }

        public void ToggleSfx(bool active)
        {
            _sfxOn = active;
            audioMixer.SetFloat("SFX", _sfxOn ? 0 : -80);
        }
    }
}