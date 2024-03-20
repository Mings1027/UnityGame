using System;
using System.Collections.Generic;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

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

        private static SoundManager _inst;
        private CancellationTokenSource _cts;
        private Camera _cam;

        private const string BGM = "BGM";
        private const string Sfx = "SFX";
        private const string Master = "Master";

        private const float MinValue = -80f;
        private const float MaxValue = 0f;

        private float _inverseMaxMinusMin;

        private Dictionary<SoundEnum, AudioClip> _musicDictionary;
        private Dictionary<SoundEnum, AudioClip> _effectDictionary;

        private AudioSource _musicSource, _effectSource;
        private byte _maxDis;

        private int _bgmVolume;
        private int _sfxVolume;

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
            _inst = this;
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

        public static void MuteBGM(bool value) => _inst._musicSource.mute = value;

        public static void PlayBGM(SoundEnum clipName)
        {
            _inst._musicSource.clip = _inst._musicDictionary[clipName];
            _inst._musicSource.Play();
        }

        public static void PlayUISound(SoundEnum clipName)
        {
            _inst._effectSource.clip = _inst._effectDictionary[clipName];
            _inst._effectSource.Play();
        }

        public static void Play3DSound(AudioClip audioClip, Vector3 position)
        {
            var distance = Vector3.Distance(_inst._cam.transform.position, position);
            if (distance > _inst._maxDis) return;
            var soundScale = Mathf.Clamp01((_inst._maxDis - distance) * _inst._inverseMaxMinusMin);
            soundScale *= 0.5f;
            _inst._effectSource.PlayOneShot(audioClip, soundScale);
        }

        public static void SetBGMVolume(float value)
        {
            var mappedValue = Mathf.Lerp(MinValue, MaxValue, value);
            _inst.audioMixer.SetFloat(BGM, mappedValue);
            PlayerPrefs.SetInt(BGM, (int)mappedValue);
        }

        public static void SetSfxVolume(float value)
        {
            var mappedValue = Mathf.Lerp(MinValue, MaxValue, value);
            _inst.audioMixer.SetFloat(Sfx, mappedValue);
            PlayerPrefs.SetInt(Sfx, (int)mappedValue);
        }

        public void SetCamera(Camera cam)
        {
            _cam = cam;
        }

        public static (float, float) GetVolume()
        {
            _inst._bgmVolume = PlayerPrefs.GetInt(BGM);
            _inst._sfxVolume = PlayerPrefs.GetInt(Sfx);

            _inst.audioMixer.SetFloat(BGM, _inst._bgmVolume);
            _inst.audioMixer.SetFloat(Sfx, _inst._sfxVolume);

            var convertBgmValue = Mathf.InverseLerp(MinValue, MaxValue, _inst._bgmVolume);
            var convertSfxValue = Mathf.InverseLerp(MinValue, MaxValue, _inst._sfxVolume);

            return (convertBgmValue, convertSfxValue);
        }

        public static async UniTaskVoid FadeOutVolume()
        {
            var volume = 0;
            _inst._cts?.Cancel();
            _inst._cts?.Dispose();
            _inst._cts = new CancellationTokenSource();
            while (volume > -80 && !_inst._cts.IsCancellationRequested)
            {
                volume--;
                await UniTask.Yield(cancellationToken: _inst._cts.Token);
                _inst.audioMixer.SetFloat(Master, volume);
            }
        }

        public static async UniTaskVoid FadeInVolume()
        {
            var lowVolume = -80;

            _inst._cts?.Cancel();
            _inst._cts?.Dispose();
            _inst._cts = new CancellationTokenSource();
            while (lowVolume < 0 && !_inst._cts.IsCancellationRequested)
            {
                lowVolume++;
                await UniTask.Yield(cancellationToken: _inst._cts.Token);
                _inst.audioMixer.SetFloat(Master, lowVolume);
            }
        }
    }
}