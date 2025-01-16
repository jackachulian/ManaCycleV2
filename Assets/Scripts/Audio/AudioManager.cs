using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private int maxSounds = 16;
        private List<AudioSource> sources = new List<AudioSource>();
        private AudioSource musicSource; 

        [Header("Sound Collections")]
        public SoundCollection boardSounds;
        public SoundCollection battleMusic;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(this);
            
        }

        void Start()
        {
            musicSource = GetComponent<AudioSource>();
            UpdateVolumes();

            GameObject newObject = new();
            AudioSource source = newObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxMixerGroup;
            
            for (int i = 0; i < maxSounds; i++)
            {
                GameObject o = Instantiate(newObject, transform);
                sources.Add(o.GetComponent<AudioSource>());
            }
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == musicSource.clip) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        /// <summary>
        /// Play random song from collection
        /// </summary>
        /// <param name="collection"></param>
        public void PlayMusic(SoundCollection collection)
        {
            PlayMusic(collection.sounds.Values.ToList()[Random.Range(0, collection.sounds.Count - 1)]);
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void PlaySound(AudioClip clip, float pitch = 1.0f, float volumeScale = 1.0f)
        {
            List<AudioSource> freeSources = sources.Where(s => !s.isPlaying).ToList();

            if (freeSources.Count == 0) return;

            AudioSource freeSource = freeSources[0];
            freeSource.pitch = pitch;
            freeSource.volume = volumeScale;
            freeSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Shorthand to play a sound specifically from the board collection
        /// </summary>
        /// <param name="key">Key from the board collection dictionary to play</param>
        /// <param name="pitch">Randomized within 5% unless specified</param>
        /// <param name="volumeScale"></param>
        public void PlaySound(string key, float pitch = -1.0f, float volumeScale = 1f)
        {
            if (pitch < 0) pitch = Random.Range(0.95f, 1.05f);
            PlaySound(boardSounds.sounds[key], pitch, volumeScale);
        }

        public void UpdateVolumes()
        {
            mixer.SetFloat("SFXVol", Mathf.Log10(PlayerPrefs.GetFloat("SFXVol") / 20 + 0.0001f) * 20);
            mixer.SetFloat("MusicVol", Mathf.Log10(PlayerPrefs.GetFloat("MusicVol") / 20 + 0.0001f) * 20);
            mixer.SetFloat("MainVol", Mathf.Log10(PlayerPrefs.GetFloat("MainVol") / 20 + 0.0001f) * 20);

        }
    }
}

