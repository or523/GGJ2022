using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton
    public static AudioManager Instance = null;

    public AudioClip m_ambience;
    
    public AudioClip m_good_event_clip;
    public AudioClip m_bad_event_clip;

    // ordered by ResourceType
    public AudioClip[] m_sfx;

    // for the memes
    public AudioClip m_human_sfx;

    [HideInInspector]
    public AudioSource m_audio;


    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAmbience()
    {
        // play ambience only on server
        if (NetworkPlayer.IsRunningOnServer())
        {
            m_audio.clip = m_ambience;
            m_audio.loop = true;
            m_audio.Play();
        }
    }

    private IEnumerator PlayAndRestore(AudioClip clip, bool should_restore_ambience)
    {
        m_audio.clip = clip;
        m_audio.loop = false;
        m_audio.Play();

        if (should_restore_ambience)
        {
            yield return new WaitForSeconds(m_audio.clip.length);
            PlayAmbience();
        }
    }

    public void PlayGoodEventClip(bool should_restore_ambience)
    {
        Debug.Log("PlayGoodEventClip");

        StartCoroutine(PlayAndRestore(m_good_event_clip, should_restore_ambience));
    }

    public void PlayBadEventClip(bool should_restore_ambience)
    {
        Debug.Log("PlayBadEventClip");

        StartCoroutine(PlayAndRestore(m_bad_event_clip, should_restore_ambience));
    }

    // only for clients
    public void PlaySFX(ResourceType type)
    {
        m_audio.clip = m_sfx[(int) type];
        m_audio.loop = false;
        m_audio.Play();
    }

    public void PlayMeme()
    {
        m_audio.clip = m_human_sfx;
        m_audio.loop = false;
        m_audio.Play();
    }
}
