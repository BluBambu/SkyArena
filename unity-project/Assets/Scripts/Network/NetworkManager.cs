﻿using System.Runtime.Remoting.Channels;
using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    private World world;

    public GameObject playerPrefab;

    public static bool hasGameStarted;

    /// <summary>
    /// The name of this game in the master server
    /// </summary>
    private string _gameInstanceName = "SkyArena";

    /// <summary>
    /// The name of the room in the master server
    /// </summary>
    private string _roomName = "Room Name";

    /// <summary>
    /// Host data of the master server
    /// </summary>
    private HostData[] _hostData;

    /// <summary>
    /// If the player has a team, if not then one will be assigned randomly on game start
    /// </summary>
    private bool hasTeam;

    /// <summary>
    /// Which team the player is apart of
    /// </summary>
    public bool isBlueTeam;

    void Start()
    {
        world = GetComponent<World>();

        hasGameStarted = false;
    }

    /// <summary>
    /// Starts the server and registers it to the master server host
    /// </summary>
    private void StartServer()
    {
        Network.InitializeServer(4, 25565, !Network.HavePublicAddress());
        MasterServer.RegisterHost(_gameInstanceName, _roomName);
    }

    /// <summary>
    /// Called automatically when a server is initialized
    /// </summary>
    private void OnServerInitialized()
    {
        Debug.Log("Server Initialized");
    }

    private void OnGUI()
    {
        if (!Network.isClient && !Network.isServer)
        {
            _roomName = GUI.TextField(new Rect(100, 130, 250, 40), _roomName, 20);
            if (GUI.Button(new Rect(400, 100, 250, 100), "Start New Server"))
                StartServer();

            if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts"))
                RefreshHostList();

            if (_hostData != null)
            {
                for (int i = 0; i < _hostData.Length; i++)
                {
                    if (GUI.Button(new Rect(400, 250 + (110 * i), 300, 100), _hostData[i].gameName))
                    {
                        JoinServer(_hostData[i]);
                    }
                }
            }
        }
        else if (Network.isServer && !hasGameStarted)
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 125, 100, 250, 100), "Start New Server"))
            {
                networkView.RPC("StartGame", RPCMode.AllBuffered);
                MasterServer.UnregisterHost();
                Network.maxConnections = -1;
            }

            if (GUI.Button(new Rect(Screen.width / 2 - 400, 200, 250, 100), "Join The Blue Team"))
                JoinBlueTeam();

            if (GUI.Button(new Rect(Screen.width / 2 + 150, 200, 250, 100), "Join The Red Team"))
                JoinRedTeam();

            if (hasTeam && isBlueTeam)
            {
                GUI.Label(new Rect(Screen.width / 2 - 65, 250, 250, 100), "You're on the blue team");
            }
            else if (hasTeam && !isBlueTeam)
            {
                GUI.Label(new Rect(Screen.width / 2 - 65, 250, 250, 100), "You're on the red team");
            }
        }
        else if (Network.isClient && !hasGameStarted)
        {
            GUI.Label(new Rect(400, 100, 250, 100), "Select a team to join until the host starts a game...");

            if (GUI.Button(new Rect(Screen.width / 2 - 400, 200, 250, 100), "Join The Blue Team"))
                JoinBlueTeam();

            if (GUI.Button(new Rect(Screen.width / 2 + 150, 200, 250, 100), "Join The Red Team"))
                JoinRedTeam();

            if (hasTeam && isBlueTeam)
            {
                GUI.Label(new Rect(Screen.width / 2 - 65, 250, 250, 100), "You're on the blue team");
            }
            else if (hasTeam && !isBlueTeam)
            {
                GUI.Label(new Rect(Screen.width / 2 - 65, 250, 250, 100), "You're on the red team");
            }
        }
    }

    private void JoinRedTeam()
    {
        hasTeam = true;
        isBlueTeam = false;
    }

    private void JoinBlueTeam()
    {
        hasTeam = true;
        isBlueTeam = true;
    }

    /// <summary>
    /// Refreshs the host list
    /// </summary>
    private void RefreshHostList()
    {
        MasterServer.RequestHostList(_gameInstanceName);
    }

    /// <summary>
    /// Automatically called when a master server event occurs
    /// </summary>
    /// <param name="masterEvent"></param>
    private void OnMasterServerEvent(MasterServerEvent masterEvent)
    {
        if (masterEvent == MasterServerEvent.HostListReceived)
        {
            _hostData = MasterServer.PollHostList();
        }
    }

    /// <summary>
    /// Automatically called on joining a master server
    /// </summary>
    private void JoinServer(HostData hostData)
    {
        Network.Connect(hostData);
    }

    /// <summary>
    /// Automatically called on connecting to a server
    /// </summary>
    private void OnConnectedToServer()
    {
    }

    [RPC]
    private void StartGame()
    {
        hasGameStarted = true;
        world.player = (Network.Instantiate(playerPrefab, transform.position, transform.rotation, 0) as GameObject).transform;
        world.InitWorld();
    }
}