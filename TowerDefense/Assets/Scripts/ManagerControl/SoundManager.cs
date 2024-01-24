using System;
using System.Collections.Generic;
using CustomEnumControl;
using UnityEngine;
using UnityEngine.Audio;
using Utilities;

namespace ManagerControl
{
    public class SoundManager : MonoBehaviour
    {
        [Serializable]
        private class EffectSound
        {
            public SoundEnum effectName;
            public AudioClip effectSource;
        }

        [Serializable]
        private class MusicSound
        {
            public SoundEnum musicName;
            public AudioClip musicClip;
        }

        private static Camera _cam;

        private const string BGMKey = "BGM";
        private const string SfxKey = "SFX";

        private static float _bgmVolume;
        private static float _inverseMaxMinusMin;

        private static Dictionary<SoundEnum, AudioClip> _musicDictionary;
        private static Dictionary<SoundEnum, AudioClip> _effectDictionary;

        private static AudioSource _musicSource, _effectSource;
        private static AudioMixer _audioMixer;
        private static byte _maxDis;

        [SerializeField] private MusicSound[] musicSounds;
        [SerializeField] private EffectSound[] effectSounds;

        [SerializeField] private AudioMixer audioMixer;
        [SerializeField, Range(0, 255)] private byte minDistance, maxDistance;

        public static bool bgmOn { get; private set; }
        public static bool sfxOn { get; private set; }

        private void OnValidate()
        {
            if (minDistance > maxDistance) minDistance = maxDistance;
            if (maxDistance < minDistance) maxDistance = minDistance;
            _maxDis = maxDistance;
        }

        private void Awake()
        {
            _audioMixer = audioMixer;
            _inverseMaxMinusMin = 1.0f / (maxDistance - minDistance);
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
            bgmOn = PlayerPrefs.GetInt(BGMKey, 1) == 1;
            sfxOn = PlayerPrefs.GetInt(SfxKey, 1) == 1;
            _maxDis = maxDistance;
        }

        private void Start()
        {
            audioMixer.SetFloat("BGM", bgmOn ? _bgmVolume : -80);
            audioMixer.SetFloat("SFX", sfxOn ? 0 : -80);
        }

        public static void MuteBGM(bool value) => _musicSource.mute = value;

        public static void PlayBGM(SoundEnum clipName)
        {
            _musicSource.clip = _musicDictionary[clipName];
            _musicSource.Play();
        }

        public static void PlayUISound(SoundEnum clipName)
        {
            _effectSource.clip = _effectDictionary[clipName];
            _effectSource.Play();
        }

        public static void Play3DSound(AudioClip audioClip, Vector3 position)
        {
            var distance = Vector3.Distance(_cam.transform.position, position);
            if (distance > _maxDis) return;
            var soundScale = Mathf.Clamp01((_maxDis - distance) * _inverseMaxMinusMin);
            soundScale *= 0.5f;
            _effectSource.PlayOneShot(audioClip, soundScale);
        }

        public static void ToggleBGM(bool active)
        {
            bgmOn = active;
            _audioMixer.SetFloat("BGM", bgmOn ? _bgmVolume : -80);
            PlayerPrefs.SetInt(BGMKey, bgmOn ? 1 : 0);
        }

        public static void ToggleSfx(bool active)
        {
            sfxOn = active;
            _audioMixer.SetFloat("SFX", sfxOn ? 0 : -80);
            PlayerPrefs.SetInt(SfxKey, sfxOn ? 1 : 0);
        }

        public void SetCamera(Camera cam)
        {
            _cam = cam;
        }
    }
}