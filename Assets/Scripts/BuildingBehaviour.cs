using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

using System;

public class BuildingBehaviour : MonoBehaviour, INetworkSerializable
{
    public String m_name = "";

    [HideInInspector]
    public ConsumerProducerBehaviour m_producer;

    // level 0 - no building
    // requirment[x] - needed resources to upgrade from x to x+1
    public int m_level;     // building level
    public Resources[] m_level_requirments;

    [HideInInspector]
    public SpriteRenderer m_renderer;

    [HideInInspector]
    public Sprite[]       m_sprites;

    // Start is called before the first frame update
    public void Start()
    {
        m_name = gameObject.name;

        m_renderer = GetComponent<SpriteRenderer>();
        m_producer = GetComponent<ConsumerProducerBehaviour>();

        String name = m_renderer.sprite.name;
        String spritepath = "Sprites/" + name.Substring(0, name.Length-2);
        Debug.Log("Loading sprites - " + spritepath);
        m_sprites = UnityEngine.Resources.LoadAll<Sprite>(spritepath);
        Debug.Log("Loaded " + m_sprites.Length + " sprites");
    }

    public bool CanUpgrade(Resources resources)
    {
        return (!IsMaxLevel() && (resources >= GetRequiredResourcesForUpgrade()));
    }

    public void Upgrade(ref Resources resources)
    {
        if (CanUpgrade(resources))
        {
            Debug.Log("Building upgraded!");
            resources -= m_level_requirments[m_level];
            ++m_level;

            m_renderer.sprite = m_sprites[m_level];
        }
    }

    public bool IsMaxLevel()
    {
        return m_level >= m_level_requirments.Length;
    }

    public Resources Produce()
    {
        return m_producer.ConsumeProduce();
    }

    public Resources GetRequiredResourcesForUpgrade()
    {
        return m_level_requirments[m_level];
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_name);
    }

    public override string ToString()
    {
        return m_name;
    }

    public void LevelScaleCalculation(ConsumerProducerBehaviour consumer)
    {
        consumer.m_calculated_consumption += (consumer.m_base_consumption * m_level);
    }
}
