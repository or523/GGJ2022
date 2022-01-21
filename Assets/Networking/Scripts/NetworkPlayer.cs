using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// This object exists for every player.
// The server can query this object and run 
public class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer LocalInstance = null;
    public NetworkVariable<bool> isReady;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsLocalPlayer)
        {
            // TODO: disable
        }
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
    void UpdatePlayerDecisionServerRPC(int decision_id, ResourceType resource, bool is_selected)
    {
        // We don't want players to get this update
        if (!IsServer)
        {
            return;
        }

        // Find GameManager and update its state
        GameManager.Instance.UpdateDecision(decision_id, resource, is_selected);
    }

    [ServerRpc]
    void UpdatePlayerIsReadyServerRPC(bool is_ready)
    {
        if (GameManager.Instance.CanPlayersReady(is_ready))
        {
            this.isReady.Value = is_ready;
        }
    }
}
