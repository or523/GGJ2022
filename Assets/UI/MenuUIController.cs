using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuUIController : MonoBehaviour
{
    // Main Menu
    public Button mainMenuServerButton;
    public Button mainMenuClientServerButton;
    public Button mainMenuExit;

    // Server
    public Button startServerButton;
    public Label serverIpLabel;
    public Label serverPlayerCountLabel;

    // Client
    public TextField clientServerIp;
    public Button clientJoinButton;
    public Label clientConnectionStatus;

    // Layouts
    public VisualTreeAsset mainMenu;
    public VisualTreeAsset serverMenu;
    public VisualTreeAsset clientMenu;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<UIDocument>().visualTreeAsset = mainMenu;

        var root = GetComponent<UIDocument>().rootVisualElement;
        mainMenuServerButton = root.Q<Button>("server-button");
        mainMenuClientServerButton = root.Q<Button>("client-button");
        mainMenuExit = root.Q<Button>("exit-button");

        mainMenuServerButton.clicked += SwitchToServerMenu;
        mainMenuClientServerButton.clicked += SwitchToClientMenu;
        mainMenuExit.clicked += ExitGame;
    }

    // This is the server hosting menu
    public void SwitchToServerMenu()
    {
        GetComponent<UIDocument>().visualTreeAsset = serverMenu;

        var root = GetComponent<UIDocument>().rootVisualElement;
        startServerButton = root.Q<Button>("start-button");
        serverIpLabel = root.Q<Label>("ip-text");
        serverPlayerCountLabel = root.Q<Label>("players-count-text");

        startServerButton.clicked += startServerButtonPressed;
    }

    // This is the client connection menu
    public void SwitchToClientMenu()
    {
        GetComponent<UIDocument>().visualTreeAsset = clientMenu;

        var root = GetComponent<UIDocument>().rootVisualElement;
        clientServerIp = root.Q<TextField>("server-ip-input");
        clientJoinButton = root.Q<Button>("join-button");
        clientConnectionStatus = root.Q<Label>("connection-status");

        clientJoinButton.clicked += ClientJoinServer;
    }

    public void ClientJoinServer()
    {
        string serverIp = clientServerIp.value;
        NetworkServer.Instance.ClientConnect(serverIp);
        clientJoinButton.SetEnabled(false);
        clientConnectionStatus.text = "Connecting...";
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    void startServerButtonPressed()
    {
        NetworkServer.Instance.StartServer();

        startServerButton.SetEnabled(false);
        serverIpLabel.text = string.Format(string.Format("Server IP: {0}", NetworkServer.Instance.LocalIPAddress()));
    }

    // Update is called once per frame
    void Update()
    {
        // Update player count
        if (serverPlayerCountLabel != null)
        {
            serverPlayerCountLabel.text = string.Format(string.Format("{0} / 4 Players", GameManager.Instance.PlayersCount));
        }
        
        // Update client connection status
        if (clientConnectionStatus != null)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                clientConnectionStatus.text = "Connected!";
            }
        }
    }

    public void SwitchToPlayerMenu()
    {
        gameObject.GetComponent<PlayerUIController>().enabled = true;
        this.enabled = false;
    }


    public void SwitchToServerGameMenu()
    {
        gameObject.GetComponent<ServerGameUIController>().enabled = true;
        this.enabled = false;
    }
}
