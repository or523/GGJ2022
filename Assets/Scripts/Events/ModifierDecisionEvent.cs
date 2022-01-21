using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "ModifierDecisionEvent", menuName = "ScriptableObjects/Modifier Event")]
public class ModifierDecisionEvent : DecisionWorldEvent
{
    public CalculationEvent m_calculation;  // the modifier

    public override void ApplyEventDecisionMade()
    {
        ResourceManagerBehaviour.Instance.UpdateResources(-m_required_resources);
    }

    public override void ApplyEventDecisionNotMade()
    {
        GameManager.Instance.SetModifier(m_calculation);
    }
}
