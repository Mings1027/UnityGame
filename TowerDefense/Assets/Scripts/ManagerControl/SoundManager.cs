using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
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

        private static CancellationTokenSource _cts;
        private static Camera _cam;

        public const string BGMKey = "BGM";
        public const string SfxKey = "SFX";

        private const float MinValue = -80f;
        private const float MaxValue = 0f;

        private static float _inverseMaxMinusMin;

        private static Dictionary<SoundEnum, AudioClip> _musicDictionary;
        private static Dictionary<SoundEnum, AudioClip> _effectDictionary;

        private static AudioSource _musicSource, _effectSource;
        private static AudioMixer _audioMixer;
        private static byte _maxDis;

        private static int _bgmVolume;
        private static int _sfxVolume;

        [SerializeField] private MusicSound[] musicSounds;
        [SerializeField] private EffectSound[] effectSounds;

        [SerializeField] private AudioMixer audioMixer;
        [SerializeField, Range(0, 255)] private byte minDistance, maxDistance;

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

            _maxDis = maxDistance;
        }

        private void OnDisable()
        {
            _cts?.Cancel();
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

        public static void SetBGMVolume(float value)
        {
            var mappedValue = Mathf.Lerp(MinValue, MaxValue, value);
            _audioMixer.SetFloat(BGMKey, mappedValue);
            PlayerPrefs.SetInt(BGMKey, (int)mappedValue);
        }

        public static void SetSfxVolume(float value)
        {
            var mappedValue = Mathf.Lerp(MinValue, MaxValue, value);
            _audioMixer.SetFloat(SfxKey, mappedValue);
            PlayerPrefs.SetInt(SfxKey, (int)mappedValue);
        }

        public void SetCamera(Camera cam)
        {
            _cam = cam;
        }

        public static (float, float) GetVolume()
        {
            _bgmVolume = PlayerPrefs.GetInt(BGMKey);
            _sfxVolume = PlayerPrefs.GetInt(SfxKey);

            _audioMixer.SetFloat(BGMKey, _bgmVolume);
            _audioMixer.SetFloat(SfxKey, _sfxVolume);

            var convertBgmValue = Mathf.InverseLerp(MinValue, MaxValue, _bgmVolume);
            var convertSfxValue = Mathf.InverseLerp(MinValue, MaxValue, _sfxVolume);

            return (convertBgmValue, convertSfxValue);
        }

        public static async UniTaskVoid FadeOutVolume(string audioType)
        {
            var volume = audioType switch
            {
                BGMKey => _bgmVolume,
                SfxKey => _sfxVolume,
                _ => 0
            };

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            while (volume > -80 && !_cts.IsCancellationRequested)
            {
                volume--;
                await UniTask.Yield(cancellationToken: _cts.Token);
                _audioMixer.SetFloat(audioType, volume);
            }
        }

        public static async UniTaskVoid FadeInVolume(string audioType)
        {
            var lowVolume = -80;
            var volume = audioType switch
            {
                BGMKey => _bgmVolume,
                SfxKey => _sfxVolume,
                _ => 0
            };

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            while (lowVolume < volume && !_cts.IsCancellationRequested)
            {
                lowVolume++;
                await UniTask.Yield(cancellationToken: _cts.Token);
                _audioMixer.SetFloat(audioType, lowVolume);
            }
        }
    }
}