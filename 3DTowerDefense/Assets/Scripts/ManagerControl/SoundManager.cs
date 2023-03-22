using GameControl;
using UnityEngine;

namespace ManagerControl
{
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
            for (var i = 0; i < SoundController.Instance.soundAudioClipArray.Length; i++)
            {
                var soundAudioClip = SoundController.Instance.soundAudioClipArray[i];
                if (soundAudioClip.sound == sound)
                    return soundAudioClip.audioClip;
            }

            return null;
        }
    }
}