using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;
using System;

public enum DecisionType
{
    Uninitialized   = -1,
    BuildingUpgrade = 0,
    Event,
}

public class Decision : INetworkSerializable
{
    public int          m_decision_id;
    public Resources    m_resources_needed;
    public Resources    m_resources_allocated;

    public bool         m_is_selected;              // is it currently selected?

    public DecisionType m_type;    

    public Decision()
    {
        m_type                  = DecisionType.Uninitialized;
        m_decision_id           = 0;
        m_resources_needed      = new Resources();
        m_resources_allocated   = new Resources();
        m_is_selected           = false;
    }    

    public void Select(ResourceType type)
    {
        m_is_selected = true;

        // update the global resource amount
        ResourceManagerBehaviour.Instance.UpdateByType(
            -m_resources_needed,
            type
        );

        // update the amount allocated for the decision
        m_resources_allocated.UpdateByType(
            -m_resources_needed,
            type
        );
    }

    public void Unselect(ResourceType type)
    {
        m_is_selected = false;

        // update the global resource amount
        ResourceManagerBehaviour.Instance.UpdateByType(
            m_resources_needed,
            type
        );

        // update the amount allocated for the decision
        m_resources_allocated.UpdateByType(
            m_resources_needed,
            type
        );
    }

    public virtual void ApplyDecision()
    {
        throw new NotImplementedException();
    }

    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_decision_id);
        m_resources_needed.NetworkSerialize(serializer);
        m_resources_allocated.NetworkSerialize(serializer);
        serializer.SerializeValue(ref m_is_selected);
        serializer.SerializeValue(ref m_type);
    }
}

public class BuildingDecision : Decision
{
    public BuildingBehaviour m_building;           // the building to upgrade

    public BuildingDecision(int id, BuildingBehaviour building) : base()
    {
        m_type              = DecisionType.BuildingUpgrade;
        m_decision_id       = id;
        m_resources_needed  = building.GetRequiredResourcesForUpgrade();
        m_building          = building;
    }

    public override void ApplyDecision()
    {
        if (m_resources_allocated >= m_resources_needed)
        {
            // resources were already updated in the global store
            // just do the decision
            m_building.Upgrade(ref ResourceManagerBehaviour.Instance.m_resources);
        }
        else
        {
            // not enough for the decision to take affect
            // reimburse
            ResourceManagerBehaviour.Instance.UpdateResources(m_resources_allocated);
        }
    }

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        m_building.NetworkSerialize(serializer);
    }
}

public class EventDecision : Decision
{
    public DecisionWorldEvent m_event;             // the event that happens

    // create a world event decision
    public EventDecision(int id, DecisionWorldEvent world_event) : base()
    {
        m_type              = DecisionType.Event;
        m_decision_id       = id;
        m_resources_needed  = world_event.GetRequiredResources();
        m_event             = world_event;
    }

    public override void ApplyDecision()
    {
        if (m_resources_allocated >= m_resources_needed)
        {
            // resources were already updated in the global store
            // do the positive part of the decision
            m_event.SetDecision(true);
            m_event.ApplyEvent();
        }
        else
        {
            // not enough for the decision to take affect
            // reimburse
            ResourceManagerBehaviour.Instance.UpdateResources(m_resources_allocated);

            // now do the negative part of the decision
            m_event.SetDecision(false);
            m_event.ApplyEvent();
        }
    }

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        m_event.NetworkSerialize(serializer);
    }
}