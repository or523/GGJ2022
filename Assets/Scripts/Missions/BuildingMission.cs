using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

[CreateAssetMenu(fileName = "BuildingMission", menuName = "Scriptable Objects/Missions/Building")]
public class BuildingMission : Mission
{
    public BuildingBehaviour m_building;

    public override bool IsMissionDone()
    {
        return m_building.IsMaxLevel();
    }

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);

        m_building.NetworkSerialize(serializer);
    }
}

