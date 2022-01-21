using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public ResourceType m_controlled_resource;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectDecision(Decision decision)
    {
        // can only select stuff that are selectable and were not selected
        if (decision.m_is_selectable && !decision.m_is_selected)
        {
            decision.Select(m_controlled_resource);            
        }
    }

    public void UnselectDecision(Decision decision)
    {
        // can only de-select stuff that are selectable and were selected
        if (decision.m_is_selectable && decision.m_is_selected)
        {
            decision.Unselect(m_controlled_resource);
        }
    }
}
