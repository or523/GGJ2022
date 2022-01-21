using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    List<GameObject> GetAllPlayersObjects()
    {
        List<GameObject> players = new List<GameObject>();
        foreach (KeyValuePair<ulong, NetworkClient> player in NetworkManager.Singleton.ConnectedClients)
        {
            players.Add(player.Value.PlayerObject.gameObject);
        }
        return players;
    }

    public void UpdateAllPlayersDecisions(List<Decision> decisions)
    {
        foreach(GameObject player in GetAllPlayersObjects())
        {
            player.GetComponent<NetworkPlayer>().UpdatePlayerDecisionsClientRpc(decisions);
        }
    }
}
