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

    public IEnumerator PlayGoodEventClip(bool should_restore_ambience=true)
    {
        m_audio.clip = m_good_event_clip;
        m_audio.loop = false;
        m_audio.Play();

        yield return new WaitForSeconds(m_audio.clip.length);
        PlayAmbience();
    }

    public IEnumerator PlayBadEventClip(bool should_restore_ambience=true)
    {
        m_audio.clip = m_bad_event_clip;
        m_audio.loop = false;
        m_audio.Play();

        if (should_restore_ambience)
        {
            yield return new WaitForSeconds(m_audio.clip.length);
            PlayAmbience();
        }
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
