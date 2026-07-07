using UnityEngine;

namespace OctoberStudio.Audio
{
    public interface IAudioManager
    {
        AudioDatabase AudioDatabase { get; }

        float SoundVolume { get; set; }
        float MusicVolume { get; set; }

        AudioSource PlaySound(AudioClip clip, float volume = 1, float pitch = 1);
        AudioSource PlaySound(int hash, float volume = 1, float pitch = 1);
        AudioSource PlaySound(AudioClipData clipData, float volume = 1, float pitch = 1);

        AudioSource PlayMusic(AudioClipData clipData);
        AudioSource PlayMusic(int hash);
    }
}