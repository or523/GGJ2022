using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum ResourceType
{
    Energy = 0,
    Food,
    Workforce,
    Wood,
    Minerals
}

[System.Serializable]
public class Resources : INetworkSerializable
{
    public int m_energy;
    public int m_food;
    public int m_workforce;
    public int m_wood;
    public int m_minerals;

    public Resources()
    {
        m_energy     = 0;
        m_food       = 0;
        m_workforce  = 0;
        m_wood       = 0;
        m_minerals   = 0;
    }

    public Resources(int energy, int food, int workforce, int wood, int minerals)
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

    // for when you want the resources to be a natural amount
    public void FixNegatives()
    {
        m_energy    = (m_energy >= 0)    ? m_energy     : 0;
        m_food      = (m_food >= 0)      ? m_food       : 0;
        m_workforce = (m_workforce >= 0) ? m_workforce  : 0;
        m_wood      = (m_wood >= 0)      ? m_wood       : 0;
        m_minerals  = (m_minerals >= 0)  ? m_minerals   : 0;
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

    // resources
    public Resources m_resources;

    void Awake()
    {
        ResourceManagerBehaviour.Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Resources delta = new Resources(5, 5, 5, 5, 5);
        UpdateResources(delta);
        UpdateResources(-delta);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateResources(Resources delta)
    {
        m_resources += delta;
        m_resources.FixNegatives();
        // Debug.Log("Delta:" + delta + ", Resources: " + m_resources);
    }

    public void UpdateByType(Resources value, ResourceType type)
    {
        m_resources.UpdateByType(value, type);
    }
}
