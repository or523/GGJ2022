using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

using System;

public enum MissionType
{
    AllTimeResourceMission,
    BuildingMission,
    GlobalResourceMission
}

[CreateAssetMenu(fileName = "Mission", menuName = "Scriptable Object/Mission")]
public class Mission : ScriptableObject, INetworkSerializable
{
    public MissionType missionType;
    public ResourceType m_type;

    public String m_display_string = "";

    // (AllTimeResourceMission / GlobalResourceMission)-specific
    public float m_value;

    // BuildingMission-specific
    public String m_building_name = "";
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
        serializer.SerializeValue(ref m_display_string);
        serializer.SerializeValue(ref m_value);
        serializer.SerializeValue(ref m_building_name);
    }

    public void SetDisplayString()
    {
        switch (missionType)
        {
            case MissionType.AllTimeResourceMission:
                m_display_string = string.Format("Produce {0} of resource {1} over the game", m_value, m_type);
                break;

            case MissionType.BuildingMission:
                m_display_string = string.Format("Upgrade {0} to the maximum level", m_building_name);
                break;

            case MissionType.GlobalResourceMission:
                m_display_string = string.Format("Have {0} of resource {1} at the end of the game", m_value, m_type);
                break;

            default:
                break;
        }
    }

    public override string ToString()
    {
        return m_display_string;
    }

}
