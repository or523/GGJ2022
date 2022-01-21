using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmBehaviour : BuildingBehaviour
{
    public int m_workforce;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // farm calculations
    public void BasicFarmCalculation(ConsumerProducerBehaviour consumer)
    {
        Resources delta = new Resources{m_food = m_workforce};
        consumer.m_calculated_consumption += delta;
    }
}
