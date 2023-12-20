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

        private const string BGMKey = "BGM";
        private const string SfxKey = "SFX";

        private float _bgmVolume;

        private Dictionary<SoundEnum, AudioClip> _musicDictionary;
        private Dictionary<SoundEnum, AudioClip> _effectDictionary;

        private AudioSource _musicSource, _effectSource;

        [SerializeField] private MusicSound[] musicSounds;
        [SerializeField] private EffectSound[] effectSounds;

        [SerializeField] private AudioMixer audioMixer;

        public bool BGMOn { get; private set; }
        public bool SfxOn { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _musicSource = transform.Find("Music Source").GetComponent<AudioSource>();
            _effectSource = transform.Find("Effect Source").GetComponent<AudioSource>();

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

            _bgmVolume = -5;
            BGMOn = PlayerPrefs.GetInt(BGMKey, 1) == 1;
            SfxOn = PlayerPrefs.GetInt(SfxKey, 1) == 1;
        }

        private void Start()
        {
            audioMixer.SetFloat("BGM", BGMOn ? _bgmVolume : -80);
            audioMixer.SetFloat("SFX", SfxOn ? 0 : -80);
        }

        public void PlayBGM(SoundEnum clipName)
        {
            _musicSource.clip = _musicDictionary[clipName];
            _musicSource.Play();
        }

        public void PlaySound(SoundEnum clipName)
        {
            _effectSource.clip = _effectDictionary[clipName];
            _effectSource.Play();
        }

        public void ToggleBGM(bool active)
        {
            BGMOn = active;
            audioMixer.SetFloat("BGM", BGMOn ? _bgmVolume : -80);
            PlayerPrefs.SetInt(BGMKey, BGMOn ? 1 : 0);
        }

        public void ToggleSfx(bool active)
        {
            SfxOn = active;
            audioMixer.SetFloat("SFX", SfxOn ? 0 : -80);
            PlayerPrefs.SetInt(SfxKey, SfxOn ? 1 : 0);
        }
    }
}