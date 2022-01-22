using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

public class NetworkServer : MonoBehaviour
{
    public static NetworkServer Instance = null;

    private void Awake()
    {
        NetworkServer.Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartServer()
    {
        Debug.Log("Attempting to start server..");
        NetworkManager.Singleton.StartServer();
    }

    public void ClientConnect(string server_ip)
    {
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = server_ip;
        NetworkManager.Singleton.StartClient();
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

    public void UpdateAllPlayersGameStarted()
    {
        // Notify all players game started
        foreach (GameObject player in GetAllPlayersObjects())
        {
            player.GetComponent<NetworkPlayer>().StartGameClientRpc();
        }

        // Change Server UI to game started
        GameObject.FindGameObjectWithTag("UIManager").GetComponent<MenuUIController>().SwitchToServerGameMenu();
        // TODO: show island & building
    }

    public void UpdateAllPlayersDecisions(List<Decision> decisions)
    {
        foreach(GameObject player in GetAllPlayersObjects())
        {
            player.GetComponent<NetworkPlayer>().UpdatePlayerDecisionsClientRpc(decisions.ToArray());
        }
    }

    public string LocalIPAddress()
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }

}
