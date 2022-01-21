using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Unity.Netcode;

public class Decision : INetworkSerializable
{
    public int          m_decision_id;
    public Resources    m_resources_needed;
    public Resources    m_resources_allocated;
    public bool         m_is_selectable;            // is the decision available?

    public bool         m_is_selected;              // is it currently selected?

    public void Select(ResourceType type)
    {
        m_is_selected = true;

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

    public void Unselect(ResourceType type)
    {
        m_is_selected = false;

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

    public void UpdateSelectable()
    {
        m_is_selectable = ResourceManagerBehaviour.Instance.m_resources >= m_resources_needed;

        // TODO - update visually
    }

    public void ApplyDecision()
    {
        if (m_resources_allocated >= m_resources_needed)
        {
            // resources were already updated in the global store
            // just do the decision

            // TODO - activate the decision
        }
        else
        {
            // not enough for the decision to take affect
            // reimburse
            ResourceManagerBehaviour.Instance.UpdateResources(m_resources_allocated);
        }
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_decision_id);
        m_resources_needed.NetworkSerialize(serializer);
        m_resources_allocated.NetworkSerialize(serializer);
        serializer.SerializeValue(ref m_is_selectable);
        serializer.SerializeValue(ref m_is_selected);
    }
}

[System.Serializable]
public class WorldEvent
{
    
}

public enum GameState
{
    WaitingForPlayers,
    GameStart,
    Produce,
    SelectDecisions,
    PlayersMove,
    WorldEvent,
    TurnEnd,
    GameEnd
};

public class GameManager : MonoBehaviour
{
    // stores all buildings - the index is used as ID for selecting the building
    [HideInInspector]
    public BuildingBehaviour[] m_buildings;

    public int m_turn_counter;

    // available events
    public WorldEvent[] m_events;

    // current decisions
    public List<Decision> m_decisions;

    // Singleton
    public static GameManager Instance = null;

    // Current Players
    public Dictionary<ulong, GameObject> m_players;

    public GameState m_gameState;

    public const int NUM_PLAYERS = 1;
    
    public int max_turns = 12;

    void Awake()
    {
        Instance = this;
        m_gameState = GameState.WaitingForPlayers;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_players = new Dictionary<ulong, GameObject>();
        m_buildings = GameObject.FindObjectsOfType<BuildingBehaviour>();

        Shuffle(m_events);
    }

    // Update is called once per frame
    void Update()
    {
        switch(m_gameState)
        {
            case GameState.WaitingForPlayers:
                if (m_players.Count == NUM_PLAYERS)
                {
                    m_gameState = GameState.GameStart;
                }
                break;

            case GameState.GameStart:
                GetComponent<NetworkServer>().UpdateAllPlayersGameStarted();
                m_gameState = GameState.Produce;
                break;

            case GameState.Produce:
                // produce resources from buildings
                Produce();
                m_gameState = GameState.SelectDecisions;
                break;

            case GameState.SelectDecisions:
                // select the available decisions for this turn
                m_decisions = SelectDecisions();
                m_gameState = GameState.PlayersMove;
                break;

            case GameState.PlayersMove:
                // let the players make their move
                if (AreAllPlayersReady())
                {
                    // TODO: commit decisions
                    m_gameState = GameState.WorldEvent;
                }
                break;

            case GameState.WorldEvent:
                // do a random world event
                WorldEvent();

                // update the turn count
                CountTurns();

                // TODO: check if turns ran out
                if (max_turns == m_turn_counter)
                {
                    m_gameState = GameState.GameEnd;
                }
                else
                {
                    m_gameState = GameState.Produce;
                }

                break;

            case GameState.GameEnd:
                // check if should end game, and end it if so need be
                EndGame();
                break;

            default:
                Debug.Log("WTF");
                break;
        }
    }

    public void Produce()
    {
        foreach (BuildingBehaviour building in m_buildings)
        {
            building.Produce();
        }

        return;
    }

    public List<Decision> SelectDecisions()
    {
        List<Decision> decisions = new List<Decision>();

        // check what buildings are upgradable with the current resources
        // loop by index since we need the building ID
        int decision_id = 0;
        for (int id=0; id<m_buildings.Length; ++id)
        {
            BuildingBehaviour building = m_buildings[id];
            if (building.CanUpgrade(ResourceManagerBehaviour.Instance.m_resources))
            {
                decisions.Add(GenerateDecision(decision_id, building));
                decision_id++;
            }
        }

        // Publish decision to clients
        GetComponent<NetworkServer>().UpdateAllPlayersDecisions(decisions);

        return decisions;
    }

    public Decision GenerateDecision(int id, BuildingBehaviour building)
    {
        return new Decision{
            m_decision_id = id,
            m_resources_needed = building.GetRequiredResourcesForUpgrade(),
        };
    }

    public bool AreAllPlayersReady()
    {
        foreach(KeyValuePair<ulong,GameObject> p in m_players)
        {
            if (!p.Value.GetComponent<NetworkPlayer>().isReady.Value)
            {
                return false;
            }
        }
        return true;
    }

    public void WorldEvent()
    {
        WorldEvent world_event  = m_events[m_turn_counter];

        // TODO - run the event

        return;
    }

    public void CountTurns()
    {
        ++m_turn_counter;
    }

    public void EndGame()
    {
        return;
    }

    public static void Shuffle<T> (T[] array)
    {
        System.Random rng = new System.Random();

        int n = array.Length;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    // update the currently selectable decisions
    public void UpdateDecisions()
    {
        foreach (Decision decision in m_decisions)
        {

        }
    }

    public void UpdateDecision(int decision_id, ResourceType resource, bool is_selected)
    {
        if (is_selected)
        {
            m_decisions[decision_id].Select(resource);
        }
        else
        {
            m_decisions[decision_id].Unselect(resource);
        }
    }

    public void AddPlayer(ulong networkId, GameObject gameObject)
    {
        m_players.Add(networkId, gameObject);
    }

    public bool CanPlayersReady(bool is_ready)
    {
        if (!is_ready)
        {
            return true;
        }

        return ResourceManagerBehaviour.Instance.m_resources.isFeasible();
    }

    public int PlayersCount
    {
        get { return m_players.Count; }
    }
}
