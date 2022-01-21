using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBehaviour : MonoBehaviour
{
    [HideInInspector]
    public ConsumerProducerBehaviour m_producer;

    // level 0 - no building
    // requirment[x] - needed resources to upgrade from x to x+1
    public int m_level;     // building level
    public Resources[] m_level_requirments;

    // Start is called before the first frame update
    public void Start()
    {
        m_producer = GetComponent<ConsumerProducerBehaviour>();
    }

    public bool CanUpgrade(Resources resources)
    {
        return (((m_level+1) < m_level_requirments.Length) 
            && (resources >= GetRequiredResourcesForUpgrade()));
    }

    public void Upgrade(ref Resources resources)
    {
        if (CanUpgrade(resources))
        {
            resources -= m_level_requirments[m_level];
            ++m_level;
        }
    }

    public void Produce()
    {
        m_producer.ConsumeProduce();
    }

    public Resources GetRequiredResourcesForUpgrade()
    {
        return m_level_requirments[m_level];
    }
}
