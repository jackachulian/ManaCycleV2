using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private int maxSounds = 16;
        private List<AudioSource> sources = new List<AudioSource>();
        private AudioSource musicSource; 

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
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

        public void PlaySound(AudioClip clip, float pitch = 1.0f)
        {
            List<AudioSource> freeSources = sources.Where(s => !s.isPlaying).ToList();

            if (freeSources.Count == 0) return;

            AudioSource freeSource = freeSources[0];
            freeSource.pitch = pitch;
            freeSource.PlayOneShot(clip);
        }

        public void UpdateVolumes()
        {
            mixer.SetFloat("SFXVol", Mathf.Log10(PlayerPrefs.GetFloat("SFXVol") / 20 + 0.0001f) * 20);
            mixer.SetFloat("MusicVol", Mathf.Log10(PlayerPrefs.GetFloat("MusicVol") / 20 + 0.0001f) * 20);
            mixer.SetFloat("MainVol", Mathf.Log10(PlayerPrefs.GetFloat("MainVol") / 20 + 0.0001f) * 20);

        }
    }
}

