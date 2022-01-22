using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModifierDecisionEvent : DecisionWorldEvent
{
    public ConsumerProducerBehaviour m_consumer;

    public Resources m_modifiers;

    public override void ApplyEventDecisionMade()
    {
        ResourceManagerBehaviour.Instance.UpdateResources(-m_required_resources);
    }

    public override void ApplyEventDecisionNotMade()
    {
        GameManager.Instance.SetModifier(m_consumer);
    }

    public void ModifierCalculation(ConsumerProducerBehaviour consumer)
    {
        consumer.m_calculated_consumption *= m_modifiers;
    }
}
