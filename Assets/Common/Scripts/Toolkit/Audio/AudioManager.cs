using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Audio
{
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        public static readonly int BUTTON_CLICK_HASH = "Button Click".GetHashCode();

        private static AudioManager instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            instance = null;
        }

        [SerializeField] AudioDatabase database;
        [SerializeField] GameObject audioSource;

        public AudioDatabase AudioDatabase => database;

        private PoolComponent<AudioSource> audioSourcePool;
        private List<AudioData> aliveSources = new List<AudioData>();

        private Dictionary<int, SoundContainer> sounds;

        private AudioSave save;

        public float SoundVolume { 
            get => save.SoundVolume;
            set
            {
                save.SoundVolume = value;
                OnSoundVolumeChanged();
            }
        }

        public float MusicVolume
        {
            get => save.MusicVolume;
            set
            {
                save.MusicVolume = value;
                OnMusicVolumeChanged();
            }
        }

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSourcePool = new PoolComponent<AudioSource>("audio source", audioSource, 2, null, true);

            gameObject.AddComponent<AudioListener>();

            sounds = new Dictionary<int, SoundContainer>();

            for(int i = 0; i < database.Sounds.Count; i++)
            {
                var sound = database.Sounds[i];
                var hash = sound.Name.GetHashCode();

                if (sounds.ContainsKey(hash))
                {
                    Debug.LogError($"Audio clip with the name {sound.Name} has already been added. You should rename one of the entries with this name.");
                } else
                {
                    sound.Init();
                    sounds.Add(hash, sound);
                }
            }

            GameController.RegisterAudioManager(this);
        }

        private void Start()
        {
            save = GameController.SaveManager.GetSave<AudioSave>("Audio");
        }

        public AudioSource PlaySound(AudioClip clip, float volume = 1, float pitch = 1)
        {
            var source = audioSourcePool.GetEntity();
            source.clip = clip;

            source.loop = false;

            source.pitch = pitch;
            source.volume = volume * save.SoundVolume;

            var data = new AudioData() { source = source, volume = volume };
            aliveSources.Add(data);

            source.Play();

            return source;
        }

        public AudioSource PlaySound(int hash, float volume = 1, float pitch = 1)
        {
            if (sounds.ContainsKey(hash))
            {
                return sounds[hash].Play(false, volume, pitch);
            }

            Debug.LogWarning($"There are no sound with hash {hash} in the AudioDatabase");
            return null;
        }

        public AudioSource PlaySound(AudioClipData clipData, float volume = 1, float pitch = 1)
        {
            var source = audioSourcePool.GetEntity();
            source.clip = clipData.AudioClip;

            source.loop = false;

            source.pitch = clipData.Pitch * pitch;
            source.volume = clipData.Volume * save.SoundVolume * volume;

            var data = new AudioData() { source = source, volume = clipData.Volume };
            aliveSources.Add(data);

            source.Play();

            return source;
        }

        public AudioSource PlayMusic(AudioClipData clipData)
        {
            var source = audioSourcePool.GetEntity();
            source.clip = clipData.AudioClip;

            source.loop = true;

            source.pitch = clipData.Pitch;
            source.volume = clipData.Volume * save.MusicVolume;

            var data = new AudioData() { source = source, volume = clipData.Volume };
            aliveSources.Add(data);

            source.Play();

            return source;
        }

        public AudioSource PlayMusic(int hash)
        {
            if (sounds.ContainsKey(hash))
            {
                return sounds[hash].Play(true);
            }

            Debug.LogWarning($"There are no sound with hash {hash} in the AudioDatabase");
            return null;
        }

        private void OnSoundVolumeChanged()
        {
            foreach(var source in aliveSources)
            {
                if (!source.source.loop)
                {
                    source.source.volume = source.volume * save.SoundVolume;
                }
            }
        }

        private void OnMusicVolumeChanged()
        {
            foreach (var source in aliveSources)
            {
                if (source.source.loop)
                {
                    source.source.volume = source.volume * save.MusicVolume;
                }
            }
        }

        private void Update()
        {
            for(int i = 0; i < aliveSources.Count; i++)
            {
                if (!aliveSources[i].source.isPlaying && !aliveSources[i].source.loop)
                {
                    aliveSources[i].source.gameObject.SetActive(false);
                    aliveSources.RemoveAt(i);
                    i--;
                }
            }
        }

        public class AudioData
        {
            public AudioSource source;
            public float volume;
        }
    }
}