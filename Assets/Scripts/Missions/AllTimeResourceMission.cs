using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

[CreateAssetMenu(fileName = "AllTimeResourceMission", menuName = "Scriptable Objects/Missions/All-Time Resource")]
public class AllTimeResourceMission : Mission
{
    public ResourceType m_type;
    public float        m_value;

    public override bool IsMissionDone()
    {
        float available = ResourceManagerBehaviour.Instance.m_alltime_total.GetByType(m_type);
        return (available >= m_value);
    }

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        
        serializer.SerializeValue(ref m_type);
        serializer.SerializeValue(ref m_value);
    }
}
