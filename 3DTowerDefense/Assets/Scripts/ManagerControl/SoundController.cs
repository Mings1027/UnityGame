using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class SoundController : Singleton<SoundController>
    {
        public SoundClip[] soundClips;

        [System.Serializable]
        public class SoundClip
        {
            public SoundManager.Sound sound;
            public AudioClip audioClip;
        }
    }

    public static class SoundManager
    {
        public enum Sound
        {
            Arrow,
            Sword,
            Spear,
            MissileShoot,
            MissileExplosion
        }

        public static void PlaySound(Sound sound, Vector3 pos)
        {
            StackObjectPool.Get<AudioSource>("Sound", pos).PlayOneShot(GetAudioClip(sound));
        }

        private static AudioClip GetAudioClip(Sound sound)
        {
            for (var i = 0; i < SoundController.Instance.soundClips.Length; i++)
            {
                var soundAudioClip = SoundController.Instance.soundClips[i];
                if (soundAudioClip.sound == sound)
                    return soundAudioClip.audioClip;
            }

            return null;
        }
    }
}