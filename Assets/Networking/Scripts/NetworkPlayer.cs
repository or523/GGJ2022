using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// This object exists for every player.
// The server can query this object and run 
public class NetworkPlayer : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Example for ClientRPC we can send to the players
    [ClientRpc]
    public void UpdatePlayerStateClientRpc(int state)
    {
        // We don't want server to get this updates
        if (IsServer || !IsLocalPlayer)
        {
            return;
        }

        // TODO: update state on the player
        Debug.Log(string.Format("Update Player State: {0}", state));
    }

    [ServerRpc]
    void UpdatePlayerDecisionServerRPC(int decision)
    {
        // We don't want players to get this update
        if (!IsServer)
        {
            return;
        }

        // TODO: Find GameManager and update its state
        Debug.Log(string.Format("Update Player Decision: {}", decision));
    }
}
