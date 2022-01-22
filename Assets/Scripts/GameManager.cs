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

    public List<ResourceType> m_remaining_resources;

    // FTW condition
    public BuildingBehaviour m_rocket;
    public Mission m_win_mission;

    public bool m_waiting_for_players;

    void Awake()
    {
        Instance = this;
        m_modifier = null;
        m_gameState = GameState.WaitingForPlayers;

        m_players = new Dictionary<ulong, GameObject>();

        m_win_mission = new Mission {
            missionType = MissionType.BuildingMission,
            m_building  = m_rocket
        };
    }

    // Start is called before the first frame update
    void Start()
    {
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
                NetworkServer.Instance.UpdateAllPlayersGameStarted();
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
                if (WaitForPlayerDecisions())
                {
                    // do a random world event
                    DoWorldEvent();
                    m_gameState = GameState.WorldEvent;
                }

                break;

            case GameState.WorldEvent:
                // clear the previous modifier
                m_modifier = null;

                // let the players make their move (in case of a decision event)
                // if no decision - this does nothing
                if (WaitForPlayerDecisions())
                {
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

    public bool WaitForPlayerDecisions()
    {
        // if not waiting for players - dont waiting :)
        if (!m_waiting_for_players)
        {
            return true;
        }

        // if waiting and all players are ready - done
        if (AreAllPlayersReady())
        {
            CommitDecisions(m_decisions);
            SetAllPlayersToNotReady();
            
            return true;
        }

        // keep on waiting...
        return false;
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

        // Update resource count
        GameObject.FindGameObjectWithTag("UIManager").GetComponent<ServerGameUIController>().UpdateResourceCounts();

        return;
    }

    public void SelectDecisions()
    {
        List<Decision> decisions = new List<Decision>();

        // check what buildings are upgradable
        // loop by index since we need the building ID
        int decision_id = 0;
        for (int id=0; id<m_buildings.Length; ++id)
        {
            BuildingBehaviour building = m_buildings[id];
            if (!building.IsMaxLevel())
            {
                Decision decision = new BuildingDecision(decision_id, building);
                decisions.Add(decision);
                decision_id++;
            }
        }

        PublishDecisions(decisions);

        // Update Server Ui
        ServerGameUIController.Instance.UpdateServerDecisions(decisions);
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

    public void SetAllPlayersToNotReady()
    {
        foreach (KeyValuePair<ulong, GameObject> p in m_players)
        {
            p.Value.GetComponent<NetworkPlayer>().isReady.Value = false;
        }
    }

    public void PublishDecision(Decision decision)
    {
        List<Decision> decisions = new List<Decision>() { decision };
        PublishDecisions(decisions);
    }

    public void PublishDecisions(List<Decision> decisions)
    {
        // Set the current server-side decisions
        m_waiting_for_players = true;
        m_decisions = decisions;

        // Publish decision to clients
        NetworkServer.Instance.UpdateAllPlayersDecisions(decisions);
    }

    public void CommitDecisions(List<Decision> decisions)
    {
        m_waiting_for_players = false;

        Debug.Log("Commiting decisions");

        // commit decisions
        foreach (Decision decision in m_decisions)
        {
            decision.ApplyDecision();
        }
    }

    public void DoWorldEvent()
    {
        WorldEvent world_event = m_events[m_turn_counter];

        switch (world_event.m_type)
        {
            case EventType.Decision:
                Debug.Log("Generating world event decision");
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

        // TODO: better represntation
        ServerGameUIController.Instance.UpdateEvent(world_event.ToString());
        
        return;
    }

    public void CountTurns()
    {
        ++m_turn_counter;
        ServerGameUIController.Instance.UpdateRoundCount();
    }

    public void EndGame()
    {
        // check which players won
        bool game_mission_done   = m_win_mission.IsMissionDone();
        bool player_mission_done = false;
        foreach(KeyValuePair<ulong,GameObject> p in m_players)
        {
            GameObject player = p.Value;
            player_mission_done = player.GetComponent<NetworkPlayer>().playerMission.IsMissionDone();
            if (game_mission_done && player_mission_done)
            {
                Debug.Log("Player " + p.Key + " Won!");
                DisplayPlayerResult(player, true);
            }
            else
            {
                Debug.Log("Player " + p.Key + " Lost!");
                DisplayPlayerResult(player, false);
            }
        }

        // play global sound depending on team victory
        if (game_mission_done)
        {
            AudioManager.Instance.PlayGoodEventClip(false);
        }
        else
        {
            AudioManager.Instance.PlayBadEventClip(false);
        }
    }

    public void DisplayPlayerResult(GameObject player, bool won)
    {
        // TODO - display on server

        // Display on player controller
        NotifyPlayerResult(player, won);
    }

    public void NotifyPlayerResult(GameObject player, bool won)
    {
        player.GetComponent<NetworkPlayer>().NotifyPlayerWonClientRpc(won);
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

        // Update resource UI
        ServerGameUIController.Instance.UpdateResourceCounts();
        ServerGameUIController.Instance.UpdateServerDecisions(m_decisions);
    }

    public void AddPlayer(ulong networkId, GameObject playerObject)
    {
        // Add to player list
        m_players.Add(networkId, playerObject);

        // Assign player resource
        System.Random random = new System.Random();
        int resource_index = random.Next(0, m_remaining_resources.Count);
        
        ResourceType player_resource = m_remaining_resources[resource_index];
        m_remaining_resources.RemoveAt(resource_index);

        playerObject.GetComponent<NetworkPlayer>().playerResource.Value = player_resource;

        // Assign player mission
        Mission player_mission = new Mission();

        // TODO: randomize all the other fields of missions
        const int NUM_MISSIONS = 3;
        int mission_index = random.Next(0, NUM_MISSIONS);
        switch (mission_index)
        {
            case 0:
                player_mission.missionType = MissionType.AllTimeResourceMission;
                player_mission.m_type = player_resource;
                break;

            case 1:
                player_mission.missionType = MissionType.BuildingMission;
                player_mission.m_type = player_resource;
                player_mission.m_building = m_buildings[(int) player_resource];
                player_mission.m_building_name = player_mission.m_building.m_name;
                break;

            case 2:
                player_mission.missionType = MissionType.GlobalResourceMission;
                player_mission.m_type = player_resource;
                break;
        }

        player_mission.SetDisplayString();

        playerObject.GetComponent<NetworkPlayer>().playerMission = player_mission;
        playerObject.GetComponent<NetworkPlayer>().UpdatePlayerMissionClientRpc(player_mission);
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
