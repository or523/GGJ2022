using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// This object exists for every player.
// The server can query this object and run 
public class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer LocalInstance = null;
    public NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);
    public NetworkVariable<ResourceType> playerResource = new NetworkVariable<ResourceType>(ResourceType.InvalidResource);
    public Mission playerMission;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsLocalPlayer)
        {
            // TODO: disable
        }
    }

    private void OnEnable()
    {
        isReady.OnValueChanged += readyValueChanged;
    }

    private void readyValueChanged(bool previousValue, bool newValue)
    {
        if (previousValue == newValue)
        {
            return;
        }

        GameObject.FindGameObjectWithTag("UIManager").GetComponent<PlayerUIController>().UpdatePlayerReadyButton(newValue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    override public void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GameManager.Instance.AddPlayer(NetworkObjectId, gameObject);
        }
        else if (IsLocalPlayer)
        {
            LocalInstance = this;
        }
    }

    // Starts the game at each player
    [ClientRpc]
    public void StartGameClientRpc()
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        GameObject.FindGameObjectWithTag("UIManager").GetComponent<MenuUIController>().SwitchToPlayerMenu();
    }

    // Example for ClientRPC we can send to the players
    [ClientRpc]
    public void UpdatePlayerDecisionsClientRpc(Decision[] decisions)
    {
        // We don't want server to get this updates
        if (IsServer || !IsLocalPlayer)
        {
            return;
        }

        // Update state on the player
        Debug.Log("Decisions updated on player");
        GameObject.FindGameObjectWithTag("UIManager").GetComponent<PlayerUIController>().UpdatePlayerDecisions(new List<Decision>(decisions));
    }

    [ServerRpc]
    public void UpdatePlayerDecisionServerRPC(int decision_id, ResourceType resource, bool is_selected)
    {
        // We don't want players to get this update
        if (!IsServer)
        {
            return;
        }

        Debug.Log(string.Format("Update player decision: {0} / {1} / {2}", decision_id, resource, is_selected));

        // Find GameManager and update its state
        GameManager.Instance.UpdateDecision(decision_id, resource, is_selected);
    }

    [ServerRpc]
    public void UpdatePlayerIsReadyServerRPC(bool is_ready)
    {
        if (GameManager.Instance.CanPlayersReady(is_ready))
        {
            this.isReady.Value = is_ready;
        }
    }

    [ClientRpc]
    public void UpdatePlayerMissionClientRpc(Mission mission)
    {
        // We don't want server to get this updates
        if (IsServer || !IsLocalPlayer)
        {
            return;
        }

        playerMission = mission;
    }

    [ClientRpc]
    public void NotifyPlayerWonClientRpc(bool won)
    {
        // We don't want server to get this updates
        if (IsServer || !IsLocalPlayer)
        {
            return;
        }

        Debug.Log("Player notified - won = " + won);

        // TODO - display this visually
    }
}
