using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

public enum EventType
{
    Decision = 0,
    Automatic,
}

[System.Serializable]
public abstract class WorldEvent : MonoBehaviour, INetworkSerializable
{
    public EventType m_type;

    // Event resolved - apply results to the world
    public abstract void ApplyEvent();

    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_type);
    }
}

[System.Serializable]
public abstract class DecisionWorldEvent : WorldEvent
{
    public bool m_was_decision_made;

    public Resources m_required_resources;

    public override void ApplyEvent()
    {
        if (m_was_decision_made)
        {
            ApplyEventDecisionMade();
            AudioManager.Instance.PlayGoodEventClip();
        }
        else
        {
            ApplyEventDecisionNotMade();
            AudioManager.Instance.PlayBadEventClip();
        }
    }

    public void SetDecision(bool was_made)
    {
        m_was_decision_made = was_made;
    }

    public abstract void ApplyEventDecisionMade();
    public abstract void ApplyEventDecisionNotMade();

    public Resources GetRequiredResources()
    {
        return m_required_resources;
    }

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);

        serializer.SerializeValue(ref m_was_decision_made);
    }
}
