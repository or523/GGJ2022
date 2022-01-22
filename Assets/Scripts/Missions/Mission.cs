using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

public enum MissionType
{
    AllTimeResourceMission,
    BuildingMission,
    GlobalResourceMission
}

public class Mission : ScriptableObject, INetworkSerializable
{
    public MissionType missionType;
    public ResourceType m_type;

    // (AllTimeResourceMission / GlobalResourceMission)-specific
    public float m_value;

    // BuildingMission-specific
    public BuildingBehaviour m_building;

    public virtual bool IsMissionDone()
    {
        switch (missionType)
        {
            case MissionType.AllTimeResourceMission:
                float available = ResourceManagerBehaviour.Instance.m_alltime_total.GetByType(m_type);
                return (available >= m_value);

            case MissionType.BuildingMission:
                return m_building.IsMaxLevel();

            case MissionType.GlobalResourceMission:
                float available2 = ResourceManagerBehaviour.Instance.m_resources.GetByType(m_type);
                return (available2 >= m_value);

            default:
                return false;
        }
    }

    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref missionType);
        serializer.SerializeValue(ref m_type);
        serializer.SerializeValue(ref m_value);
    }

    public override string ToString()
    {
        return string.Format("{0}, resource: {1}, amount: {2}", missionType, m_type, m_value);
    }

}
