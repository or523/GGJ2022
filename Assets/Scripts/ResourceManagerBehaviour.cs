using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum ResourceType
{
    Energy = 0,
    Food,
    Wood,
    Minerals,
    Workforce,
    InvalidResource
}

[System.Serializable]
public class Resources : INetworkSerializable
{
    public float m_energy;
    public float m_food;
    public float m_workforce;
    public float m_wood;
    public float m_minerals;

    public Resources()
    {
        m_energy     = 0;
        m_food       = 0;
        m_workforce  = 0;
        m_wood       = 0;
        m_minerals   = 0;
    }

    public Resources(float energy, float food, float workforce, float wood, float minerals)
    {
        m_energy     = energy;
        m_food       = food;
        m_workforce  = workforce;
        m_wood       = wood;
        m_minerals   = minerals;
    }

    public static Resources operator +(Resources a, Resources b)
    {
        Resources summed = new Resources();
        
        summed.m_energy     = a.m_energy    + b.m_energy;
        summed.m_food       = a.m_food      + b.m_food;
        summed.m_workforce  = a.m_workforce + b.m_workforce;
        summed.m_wood       = a.m_wood      + b.m_wood;
        summed.m_minerals   = a.m_minerals  + b.m_minerals;

        return summed;
    }

    public static Resources operator *(Resources a, Resources b)
    {
        Resources multiplied = new Resources();
        
        multiplied.m_energy     = a.m_energy    * b.m_energy;
        multiplied.m_food       = a.m_food      * b.m_food;
        multiplied.m_workforce  = a.m_workforce * b.m_workforce;
        multiplied.m_wood       = a.m_wood      * b.m_wood;
        multiplied.m_minerals   = a.m_minerals  * b.m_minerals;

        return multiplied;
    }

    public static Resources operator -(Resources a) 
    {
        Resources neg = new Resources();
        
        neg.m_energy     = -a.m_energy;
        neg.m_food       = -a.m_food;
        neg.m_workforce  = -a.m_workforce;
        neg.m_wood       = -a.m_wood;
        neg.m_minerals   = -a.m_minerals;

        return neg;
    }

    public static Resources operator -(Resources a, Resources b) => a + (-b);

    /* comparison operators - note that this does not induce a partial ordering! */
    public static bool operator >=(Resources a, Resources b)
    {
        return (
            (a.m_energy     >= b.m_energy)       &&
            (a.m_food       >= b.m_food)         &&
            (a.m_workforce  >= b.m_workforce)    &&
            (a.m_wood       >= b.m_wood)         &&
            (a.m_minerals   >= b.m_minerals)
            );
    }

    public static bool operator <=(Resources a, Resources b) => (b >= a);

    public override string ToString()
    {
        return "Energy = "  + m_energy    + ", " +
        "Food = "           + m_food      + ", " +
        "Workforce = "      + m_workforce + ", " +
        "Wood = "           + m_wood      + ", " +
        "Minerals = "       + m_minerals;
    }


    public void UpdateByType(Resources value, ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Energy:
                m_energy += value.m_energy;
                break;

            case ResourceType.Food:
                m_food += value.m_food;
                break;

            case ResourceType.Workforce:
                m_workforce += value.m_workforce;
                break;

            case ResourceType.Wood:
                m_wood += value.m_wood;
                break;

            case ResourceType.Minerals:
                m_minerals += value.m_minerals;
                break;
        }
    }

    public float GetByType(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Energy:
                return this.m_energy;

            case ResourceType.Food:
                return this.m_food;

            case ResourceType.Workforce:
                return this.m_workforce;

            case ResourceType.Wood:
                return this.m_wood;

            case ResourceType.Minerals:
                return this.m_minerals;

            default:
                Debug.Log("What the...");
                return 0;
        }
    }    

    // for when you want the resources to be a natural amount
    public void FixNegatives()
    {
        m_energy    = (m_energy >= 0)    ? m_energy     : 0;
        m_food      = (m_food >= 0)      ? m_food       : 0;
        m_workforce = (m_workforce >= 0) ? m_workforce  : 0;
        m_wood      = (m_wood >= 0)      ? m_wood       : 0;
        m_minerals  = (m_minerals >= 0)  ? m_minerals   : 0;
    }

    public void FixFractions()
    {
        m_energy    = Mathf.Floor(m_energy);
        m_food      = Mathf.Floor(m_food);
        m_workforce = Mathf.Floor(m_workforce);
        m_wood      = Mathf.Floor(m_wood);
        m_minerals  = Mathf.Floor(m_minerals);
    }

    public void Fix()
    {
        FixNegatives();
        FixFractions();
    }

    public bool isFeasible()
    {
        return this >= new Resources();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_energy);
        serializer.SerializeValue(ref m_food);
        serializer.SerializeValue(ref m_workforce);
        serializer.SerializeValue(ref m_wood);
        serializer.SerializeValue(ref m_minerals);
    }
}

public class ResourceManagerBehaviour : MonoBehaviour
{
    // singleton
    static public ResourceManagerBehaviour Instance {get; private set;}

    // current resources
    public Resources m_resources;

    // total produced
    public Resources m_alltime_total;

    void Awake()
    {
        ResourceManagerBehaviour.Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateResources(Resources delta)
    {
        // update the current resources
        m_resources += delta;
        m_resources.Fix();
        
        // update the totals - only take the positive values of delta
        delta.Fix();
        m_alltime_total += delta;
    }

    public void UpdateByType(Resources value, ResourceType type)
    {
        // update the current resources
        m_resources.UpdateByType(value, type);
        m_resources.Fix();

        // update the totals - only take the positive values of delta
        value.Fix();
        m_alltime_total.UpdateByType(value, type);
    }
}
