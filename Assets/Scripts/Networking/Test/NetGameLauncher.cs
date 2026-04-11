using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;
using Networking;

public class NetGameLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("UI")]
    [SerializeField] private GameObject netPanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    [Header("Network")]
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private NetworkSceneManagerDefault sceneManager;
    [SerializeField] private string sessionName = "Coop_Test";

    [SerializeField] private NetPlayerSpawner playerSpawner;

    private bool leftMousePressed;
    private bool rightMousePressed;

    private void Awake()
    {
        if (runner != null)
            runner.AddCallbacks(this);

        if (hostButton != null)
            hostButton.onClick.AddListener(StartHost);

        if (joinButton != null)
            joinButton.onClick.AddListener(StartClient);
    }

    private async void StartHost()
    {
        hostButton.interactable = false;
        joinButton.interactable = false;

        runner.ProvideInput = true;

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"Host error: {result.ShutdownReason} / {result.ErrorMessage}");
            hostButton.interactable = true;
            joinButton.interactable = true;
        }
    }

    private async void StartClient()
    {
        hostButton.interactable = false;
        joinButton.interactable = false;

        runner.ProvideInput = true;

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"Client error: {result.ShutdownReason} / {result.ErrorMessage}");
            hostButton.interactable = true;
            joinButton.interactable = true;
        }
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server.");
        if (netPanel != null)
            netPanel.SetActive(false);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player joined: {player.PlayerId}");

        if (runner.IsServer && playerSpawner != null)
        {
            playerSpawner.SpawnPlayer(player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player left: {player.PlayerId}");

        if (runner.IsServer && playerSpawner != null)
        {
            playerSpawner.DespawnPlayer(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetInputData data = new NetInputData();

        Vector2 move = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) move.y += 1f;
        if (Input.GetKey(KeyCode.S)) move.y -= 1f;
        if (Input.GetKey(KeyCode.A)) move.x -= 1f;
        if (Input.GetKey(KeyCode.D)) move.x += 1f;

        data.move = move.normalized;

        if (Input.GetMouseButtonDown(0))
            leftMousePressed = true;

        if (Input.GetMouseButtonDown(1))
            rightMousePressed = true;

        data.buttons.Set(NetInputData.MOUSE_LEFT, leftMousePressed);
        data.buttons.Set(NetInputData.MOUSE_RIGHT, rightMousePressed);
        data.buttons.Set(NetInputData.JUMP, Input.GetKey(KeyCode.Space));
        data.buttons.Set(NetInputData.DASH, Input.GetKey(KeyCode.LeftShift));
        data.buttons.Set(NetInputData.RUN, Input.GetKey(KeyCode.LeftAlt));

        input.Set(data);

        leftMousePressed = false;
        rightMousePressed = false;
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.LogWarning($"Disconnected from server: {reason}");
    }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

}