using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace OctoberStudio.Audio
{
    [CreateAssetMenu(fileName = "Audio Database", menuName = "October/Audio Database")]
    public class AudioDatabase : ScriptableObject
    {
        [SerializeField] List<SoundContainer> sounds;
        public List<SoundContainer> Sounds => sounds;

        [Header("Music (Obsolete. Use Sounds instead)")]
        [Tooltip("This value is obsolete and will be deprecated. Use sounds instead")] 
        [SerializeField] SoundContainer music;

        [System.Obsolete("This Property is obsolete and will be deprecated. Use sounds instead")]
        public SoundContainer Music => music;

        public void Init()
        {
            if(music != null) music.Init();
        }
    }

    [System.Serializable]
    public class SoundContainer
    {
        [SerializeField] string name;
        public string Name => name;

        [Tooltip("The minimum interval between sounds")]
        [FormerlySerializedAs("coodlown")]
        [SerializeField, Range(0, 1f)] float cooldown;

        [SerializeField] AudioClipData audioClip;

        private float lastTimePlayed;

        public void Init()
        {
            lastTimePlayed = -1;
        }

        public AudioSource Play(bool music = false, float volume = 1, float pitch = 1)
        {
            if(cooldown == 0 || Time.unscaledTime > lastTimePlayed + cooldown)
            {
                lastTimePlayed = Time.unscaledTime;

                if (music)
                {
                    return GameController.AudioManager.PlayMusic(audioClip);
                }
                else
                {
                    return GameController.AudioManager.PlaySound(audioClip, volume, pitch);
                }
            }

            return null;
        }
    }

    [System.Serializable]
    public class SoundsContainer
    {
        [Tooltip("The minimum interval between sounds")]
        [FormerlySerializedAs("coodlown")]
        [SerializeField, Range(0, 1f)] float cooldown;

        [SerializeField] List<AudioClipData> audioClips;

        private float lastTimePlayed;

        public void Init()
        {
            lastTimePlayed = -1;
        }

        public void Play(bool music = false)
        {
            if (lastTimePlayed == 0 || Time.unscaledTime > lastTimePlayed + cooldown)
            {
                var audioClip = audioClips.Random();
                if(music)
                {
                    GameController.AudioManager.PlayMusic(audioClip);
                } else
                {
                    GameController.AudioManager.PlaySound(audioClip);
                }
                lastTimePlayed = Time.unscaledTime;
            }
        }
    }

    [System.Serializable]
    public class AudioClipData
    {
        [SerializeField] AudioClip audioClip;
        public AudioClip AudioClip => audioClip;

        [SerializeField] Vector2 volume = Vector2.one;
        public float Volume => Random.Range(volume.x, volume.y);

        [SerializeField] Vector2 pitch = Vector2.one;
        public float Pitch => Random.Range(pitch.x, pitch.y);
    }
}