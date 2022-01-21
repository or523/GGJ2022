using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

public class TestNetworkScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // This is a client who want to connect
        if (Input.GetKeyUp(KeyCode.C))
        {
            // TODO: fill with actual server address
        }
        // This is a server starting
        else if (Input.GetKeyUp(KeyCode.S))
        {
        }
        // Send Client RPC
        else if (Input.GetKeyUp(KeyCode.Z))
        {
            // Iterate over all players
            foreach(KeyValuePair<ulong, NetworkClient> player in NetworkManager.Singleton.ConnectedClients)
            {
                //player.Value.PlayerObject.gameObject.GetComponent<NetworkPlayer>().UpdatePlayerStateClientRpc((int) player.Key);
            }
        }
    }
}
