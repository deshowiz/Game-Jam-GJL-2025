using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private int maxSFXSources = 10;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource[] sfxSources;
    
    [Header("Audio Clips")]
    public AudioClip[] musicClips;
    public AudioClip[] sfxClips;
    
    private Dictionary<string, AudioClip> audioClipDict = new Dictionary<string, AudioClip>();
    private int currentSFXIndex = 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
     private void InitializeAudioManager()
    {
        if (musicSource == null)
        {
            GameObject musicObject = new GameObject("MusicSource");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        sfxSources = new AudioSource[maxSFXSources];
        for (int i = 0; i < maxSFXSources; i++)
        {
            GameObject sfxObject = new GameObject($"SFXSource_{i}");
            sfxObject.transform.SetParent(transform);
            sfxSources[i] = sfxObject.AddComponent<AudioSource>();
            sfxSources[i].playOnAwake = false;
        }
        
        PopulateAudioDictionary();
    }
    
    private void PopulateAudioDictionary()
    {
        foreach (AudioClip clip in musicClips)
        {
            if (clip != null && !audioClipDict.ContainsKey(clip.name))
            {
                audioClipDict.Add(clip.name, clip);
            }
        }

        foreach (AudioClip clip in sfxClips)
        {
            if (clip != null && !audioClipDict.ContainsKey(clip.name))
            {
                audioClipDict.Add(clip.name, clip);
            }
        }
    }
    
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }
    
    public void PlaySFX(string clipName, float volume = 1f)
    {
        if (audioClipDict.TryGetValue(clipName, out AudioClip clip))
        {
            PlaySFX(clip, volume);
        }
        else
        {
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
        }
    }
    
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            AudioSource availableSource = GetAvailableSFXSource();
            availableSource.clip = clip;
            availableSource.volume = volume;
            availableSource.Play();
        }
    }
    
    public void StopAllSFX()
    {
        foreach (AudioSource source in sfxSources)
        {
            source.Stop();
        }
    }
    
    private AudioSource GetAvailableSFXSource()
    {
        for (int i = 0; i < sfxSources.Length; i++)
        {
            if (!sfxSources[i].isPlaying)
            {
                return sfxSources[i];
            }
        }

        AudioSource source = sfxSources[currentSFXIndex];
        currentSFXIndex = (currentSFXIndex + 1) % sfxSources.Length;
        return source;
    }
}
