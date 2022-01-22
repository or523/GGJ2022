using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Unity.Netcode;

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
    public WorldEvent   m_nop_event;    // event that does nothing, to pad to the number of game turns

    // current decisions
    public List<Decision> m_decisions;

    // Singleton
    public static GameManager Instance = null;

    // Current Players
    public Dictionary<ulong, GameObject> m_players;

    public GameState m_gameState;

    public const int NUM_PLAYERS = 1;
    
    public int max_turns;

    public ConsumerProducerBehaviour m_modifier;

    // mission templates
    public Mission[] m_mission_templates;

    public List<ResourceType> m_remaining_resources;

    void Awake()
    {
        Instance = this;
        m_modifier = null;
        m_gameState = GameState.WaitingForPlayers;

        m_players = new Dictionary<ulong, GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_players = new Dictionary<ulong, GameObject>();
        m_buildings = GameObject.FindObjectsOfType<BuildingBehaviour>();
        m_remaining_resources = new List<ResourceType> { ResourceType.Energy, ResourceType.Food, ResourceType.Minerals, ResourceType.Wood };

        int num_events_to_pad = max_turns - m_events.Length;
        if (num_events_to_pad > 0)
        {
            // create padded array
            WorldEvent[] concat = new WorldEvent[max_turns];

            // add the padding events
            for (int i = 0; i < num_events_to_pad; ++i)
            {
                concat[i] = m_nop_event;
            }

            // copy the original events in
            m_events.CopyTo(concat, num_events_to_pad);

            // replace the events list
            m_events = concat;
        }

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
                SelectDecisions();
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
                // clear the previous modifier
                m_modifier = null;

                // do a random world event
                DoWorldEvent();

                // update the turn count
                CountTurns();

                // check if turns ran out
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
        Resources production = new Resources();
        foreach (BuildingBehaviour building in m_buildings)
        {
            production = building.Produce();
        }

        // if there is an active modifier - apply
        if (null != m_modifier)
        {
            m_modifier.m_base_consumption = production;
            production = m_modifier.ConsumeProduce();
        }

        ResourceManagerBehaviour.Instance.UpdateResources(production);

        return;
    }

    public void SelectDecisions()
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
                Decision decision = new BuildingDecision(decision_id, building);
                decisions.Add(decision);
                decision_id++;
            }
        }

        PublishDecisions(decisions);
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

    public void PublishDecision(Decision decision)
    {
        List<Decision> decisions = new List<Decision>() { decision };
        PublishDecisions(decisions);
    }

    public void PublishDecisions(List<Decision> decisions)
    {
        // Set the current server-side decisions
        m_decisions = decisions;

        // Publish decision to clients
        GetComponent<NetworkServer>().UpdateAllPlayersDecisions(decisions);
    }

    public void DoWorldEvent()
    {
        WorldEvent world_event = m_events[m_turn_counter];

        switch (world_event.m_type)
        {
            case EventType.Decision:
                Decision decision = new EventDecision(0, world_event as DecisionWorldEvent);
                PublishDecision(decision);
                break;

            case EventType.Automatic:
                world_event.ApplyEvent();
                break;

            default:
                Debug.Log("What are we doing here?");
                break;
        }
        
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

    public void AddPlayer(ulong networkId, GameObject playerObject)
    {
        m_players.Add(networkId, playerObject);

        System.Random random = new System.Random();
        int resource_index = random.Next(0, m_remaining_resources.Count);
        
        ResourceType player_resource = m_remaining_resources[resource_index];
        m_remaining_resources.RemoveAt(resource_index);

        playerObject.GetComponent<NetworkPlayer>().playerResource.Value = player_resource;
    }

    public bool CanPlayersReady(bool is_ready)
    {
        if (!is_ready)
        {
            return true;
        }

        return ResourceManagerBehaviour.Instance.m_resources.isFeasible();
    }

    public void SetModifier(ConsumerProducerBehaviour modifier)
    {
        m_modifier = modifier;
    }
    
    public int PlayersCount
    {
        get { return m_players.Count; }
    }
}
