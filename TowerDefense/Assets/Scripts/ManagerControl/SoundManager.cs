using System;
using System.Collections.Generic;
using CustomEnumControl;
using GameControl;
using UnityEngine;
using UnityEngine.Audio;

namespace ManagerControl
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _inst;

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

        private void Awake()
        {
            _inst = this;
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

        private void Start()
        {
            _bgmOn = _sfxOn = true;
        }

        public static void PlayBGM(SoundEnum clipName) => _inst.InstPlayBGM(clipName);

        private void InstPlayBGM(SoundEnum clipName)
        {
            musicSource.clip = _musicDictionary[clipName];
            musicSource.Play();
        }

        public static void PlaySound(SoundEnum clipName) => _inst.InstPlaySound(clipName);

        private void InstPlaySound(SoundEnum clipName)
        {
            effectSource.clip = _effectDictionary[clipName];
            effectSource.Play();
        }

        public static void ToggleBGM() => _inst.InstToggleBGM();

        private void InstToggleBGM()
        {
            _bgmOn = !_bgmOn;
            audioMixer.SetFloat("BGM", _bgmOn ? 0 : -80);
        }

        public static void ToggleSfx() => _inst.InstToggleSfx();

        private void InstToggleSfx()
        {
            _sfxOn = !_sfxOn;
            audioMixer.SetFloat("SFX", _sfxOn ? 0 : -80);
        }

        public bool BGMToggle()
        {
            if (_bgmOn)
            {
                _bgmOn = false;
                musicSource.mute = true;
            }
            else
            {
                _bgmOn = true;
                musicSource.mute = false;
            }

            return _bgmOn;
        }
    }
}