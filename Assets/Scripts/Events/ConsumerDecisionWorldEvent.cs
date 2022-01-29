using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// events that consume resources either way
[System.Serializable]
public class ConsumerDecisionWorldEvent : DecisionWorldEvent
{
    public Resources m_penalty_resources;

    public override void ApplyEventDecisionMade()
    {
        ResourceManagerBehaviour.Instance.UpdateResources(-m_required_resources);
    }

    public override void ApplyEventDecisionNotMade()
    {
        ResourceManagerBehaviour.Instance.UpdateResources(-m_penalty_resources, true);
    }
}
