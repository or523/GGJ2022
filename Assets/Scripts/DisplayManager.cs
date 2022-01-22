using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class DisplayManager : MonoBehaviour
{
    public String m_island_anim_path;

    public Sprite[]       m_sprites;

    public SpriteRenderer m_renderer;

    public float m_frame_delay;

    public bool m_is_playing;
    public int m_frame;

    public float m_time_since_frame;

    public GameObject[] m_buildings;

    // Singleton
    public static DisplayManager Instance = null;

     void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_sprites = UnityEngine.Resources.LoadAll<Sprite>(m_island_anim_path);

        Array.Sort(m_sprites, (x, y) => Int32.Parse(x.name) - Int32.Parse(y.name));

        m_renderer = GetComponent<SpriteRenderer>();
        m_frame = 0;

        ToggleBuildings(false);
        PlayAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_is_playing)
        {
            m_time_since_frame += Time.deltaTime;
            if (m_time_since_frame > m_frame_delay)
            {
                m_time_since_frame -= m_frame_delay;   
                m_renderer.sprite = m_sprites[m_frame];
                m_frame = (m_frame + 1) % m_sprites.Length;
            }
        }
    }

    public void ToggleBuildings(bool show)
    {
        foreach (GameObject building in m_buildings)
        {
            building.SetActive(show);
        }
    }

    public void PlayAnimation()
    {
        m_is_playing = true;
        m_time_since_frame = m_frame_delay;
    }

    public void StopAnimation()
    {
        m_is_playing = false;
        m_time_since_frame = m_frame_delay;
    }
}
