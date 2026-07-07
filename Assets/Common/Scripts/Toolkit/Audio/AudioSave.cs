using OctoberStudio.Save;
using UnityEngine;

namespace OctoberStudio.Audio
{
    public class AudioSave : ISave
    {
        [SerializeField] float soundVolume = 1f;
        [SerializeField] float musicVolume = 1f;

        public float SoundVolume { get => soundVolume; set => soundVolume = value; }
        public float MusicVolume { get => musicVolume; set => musicVolume = value; }

        public void Flush()
        {

        }
    }
}