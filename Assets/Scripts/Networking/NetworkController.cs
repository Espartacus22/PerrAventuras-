using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Networking;

public class NetworkController : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;

    [Header("Network Components")]
    [SerializeField] private NetworkRunner _networkRunner;
    [SerializeField] private NetworkSceneManagerDefault _networkSceneManagerDefault;
    [SerializeField] private NetworkObject _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>(); 
    
    private bool _mouseButton1Pressed;
    private bool _mouseButton2Pressed;

    private void Start()
    {
        _createRoomButton.onClick.AddListener(CreateRoom);
        _joinRoomButton.onClick.AddListener(JoinRoom);
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
            {

            _mouseButton1Pressed = true;

            }



        if (Input.GetMouseButtonDown(1))
        {

            _mouseButton2Pressed = true;

        }
    }

    private async void CreateRoom()
    {
        var gameArg = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = "Room_01",
            SceneManager = _networkSceneManagerDefault
        };

        var result = await _networkRunner.StartGame(gameArg);

        if (!result.Ok)
        {
            Debug.LogError($"Failed to create room: {result.ShutdownReason}");
            Debug.LogError($"Error: {result.ErrorMessage}");
        }
    }

    private async void JoinRoom()
    {
        var gameArg = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = "Room_01",
            SceneManager = _networkSceneManagerDefault
        };

        var result = await _networkRunner.StartGame(gameArg);

        if (!result.Ok)
        {
            Debug.LogError(result.ShutdownReason);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("OnPlayerJoined");
        _lobbyPanel.SetActive(false);

        if (!_networkRunner.IsServer) return;

        var playerSpawned = _networkRunner.Spawn(_playerPrefab, new Vector3(UnityEngine.Random.Range(-3, 3), 0, 0), Quaternion.identity, player);
        _players.Add(player, playerSpawned);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!_networkRunner.IsServer) return;
        if (_players.Remove(player, out var playerSpawned)) 
        {
            _networkRunner.Despawn(playerSpawned);  
        }
       
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

        var inputPlayer = new NetworkInputPlayer();

        if (Input.GetKey(KeyCode.W))
        {
            inputPlayer.moveDirection += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            inputPlayer.moveDirection += Vector3.back;
        }

        if (Input.GetKey(KeyCode.A))
        {
            inputPlayer.moveDirection += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D))
        {
            inputPlayer.moveDirection += Vector3.right;
        }
        inputPlayer.buttons.Set(NetworkInputPlayer.MOUSE_BUTTON_0, _mouseButton1Pressed);
        inputPlayer.buttons.Set(NetworkInputPlayer.MOUSE_BUTTON_1, _mouseButton2Pressed);
        input.Set(inputPlayer); 

        _mouseButton1Pressed = false;
        _mouseButton2Pressed = false;
    }


    //-----------------------------------------------------------------------------------//
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}

