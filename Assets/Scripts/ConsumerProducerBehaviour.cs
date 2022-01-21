using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

[System.Serializable]
public class CalculationEvent : UnityEvent<ConsumerProducerBehaviour>
{
}

public class ConsumerProducerBehaviour : MonoBehaviour
{
    public Resources m_base_consumption;
    public CalculationEvent m_calculation;

    // changes when the calculation event is invoked
    [HideInInspector]
    public Resources m_calculated_consumption;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Strategy functions - write more of these for different types of calculations
    // Always end the event list with a Producer/Consumer Calculation
    public void IdentityCalculation(ConsumerProducerBehaviour consumer)
    {
        consumer.m_calculated_consumption = consumer.m_base_consumption;
    }

    public void ConsumerCalculation(ConsumerProducerBehaviour consumer)
    {
        consumer.m_calculated_consumption = -consumer.m_calculated_consumption;
    }

    public void ProducerCalculation(ConsumerProducerBehaviour consumer)
    {
        // do nothing
    }

    public void ConsumeProduce()
    {
        m_calculation.Invoke(this);
        // Debug.Log("Calculated = " + m_calculated_consumption);
        ResourceManagerBehaviour.Instance.UpdateResources(m_calculated_consumption);
    }
}
