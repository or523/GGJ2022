using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

/*
 * For a given resource type (e.g. food), have the given amount of resource
 * present in the pool at the end of the game
 */
[CreateAssetMenu(fileName = "GlobalResourceMission", menuName = "Scriptable Objects/Missions/Global Resource")]
public class GlobalResourceMission : Mission
{
    public ResourceType m_type;
    public float        m_value;

    public override bool IsMissionDone()
    {
        float available = ResourceManagerBehaviour.Instance.m_resources.GetByType(m_type);
        return (available >= m_value);
    }

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);

        serializer.SerializeValue(ref m_type);
        serializer.SerializeValue(ref m_value);
    }
}
