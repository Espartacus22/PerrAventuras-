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

    [Header("Pickups (spawneados por server al crear sala)")]
    [SerializeField] private NetworkObject[] _pickupPrefabs;     
    [SerializeField] private int _pickupAmount = 25;
    [SerializeField] private float _pickupRadius = 12f;

    private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();
    private bool _mouseButton1Pressed;
    private bool _mouseButton2Pressed;
    private bool _pickupsSpawned = false;   

    private void Start()
    {
        _createRoomButton.onClick.AddListener(CreateRoom);
        _joinRoomButton.onClick.AddListener(JoinRoom);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) _mouseButton1Pressed = true;
        if (Input.GetMouseButtonDown(1)) _mouseButton2Pressed = true;
    }

    private async void CreateRoom()
    {
        Debug.Log("[NETWORK] 1. Creando sala como Host...");

        try
        {
            var gameArg = new StartGameArgs()
            {
                GameMode = GameMode.Host,
                SessionName = "Room_01",
                SceneManager = _networkSceneManagerDefault,
                Scene = SceneRef.FromIndex(3)
            };

            Debug.Log("[NETWORK] 2. Argumentos listos. Esperando a Fusion...");

            var result = await _networkRunner.StartGame(gameArg);

            Debug.Log("[NETWORK] 3. Fusion terminó el proceso.");

            if (!result.Ok)
            {
                Debug.LogError($"[NETWORK ERROR] Falló al crear la sala: {result.ShutdownReason}");
                return;
            }

            Debug.Log("[NETWORK] 4. ¡SALA CREADA CON ÉXITO! Cargando escena...");

            // Spawn de items (tu lógica original)
            if (_networkRunner.IsServer && !_pickupsSpawned)
            {
                _pickupsSpawned = true;
                if (_pickupPrefabs != null && _pickupPrefabs.Length > 0)
                {
                    Debug.Log($"[SPAWN] Spawneando {_pickupAmount} items...");
                    for (int i = 0; i < _pickupAmount; i++)
                    {
                        int randomIndex = Random.Range(0, _pickupPrefabs.Length);
                        NetworkObject prefab = _pickupPrefabs[randomIndex];
                        Vector3 pos = new Vector3(Random.Range(-_pickupRadius, _pickupRadius), 0.5f, Random.Range(-_pickupRadius, _pickupRadius));
                        _networkRunner.Spawn(prefab, pos, Quaternion.identity);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NETWORK CRASH] Hubo un error crítico: {e.Message}\n{e.StackTrace}");
        }
    }

    private async void JoinRoom()
    {
        var gameArg = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = "Room_01",
            SceneManager = _networkSceneManagerDefault,
            Scene = SceneRef.FromIndex(3) 
        };
        var result = await _networkRunner.StartGame(gameArg);
        if (!result.Ok)
        {
            Debug.LogError(result.ShutdownReason);
        }
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        if (runner.IsServer && !_pickupsSpawned)
        {
            _pickupsSpawned = true;
            for (int i = 0; i < _pickupAmount; i++)
            {
                var prefab = _pickupPrefabs[Random.Range(0, _pickupPrefabs.Length)];
                Vector3 pos = new Vector3(
                    Random.Range(-_pickupRadius, _pickupRadius),
                    0.5f,
                    Random.Range(-_pickupRadius, _pickupRadius)
                );
                runner.Spawn(prefab, pos, Quaternion.identity);
            }
            Debug.Log($"[SERVER] Spawned {_pickupAmount} pickups!");
        }
    }

    private void SpawnPickups(NetworkRunner runner)
    {
        for (int i = 0; i < _pickupAmount; i++)
        {
            var prefab = _pickupPrefabs[Random.Range(0, _pickupPrefabs.Length)];
            Vector3 pos = new Vector3(
                Random.Range(-_pickupRadius, _pickupRadius),
                0.5f,
                Random.Range(-_pickupRadius, _pickupRadius)
            );
            runner.Spawn(prefab, pos, Quaternion.identity);
        }
        Debug.Log($"[SERVER] Spawned {_pickupAmount} pickups al crear la sala!");



    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("OnPlayerJoined");

        // ESCUDO ANTI-CRASH: Solo apagamos la UI si todavía existe en la escena
        if (_lobbyPanel != null)
        {
            _lobbyPanel.SetActive(false);
        }

        if (!runner.IsServer) return;

        // El servidor spawnea el player y le asigna InputAuthority al cliente
        var playerSpawned = runner.Spawn(_playerPrefab,
            new Vector3(Random.Range(-3f, 3f), 3f, Random.Range(-3f, 3f)), // Lo subí a 3f en Y para que no caiga por el piso
            Quaternion.identity, player);

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

        // 1. Ejes de Movimiento
        inputPlayer.moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // 2. Botones de Acción
        inputPlayer.buttons.Set(NetworkInputPlayer.MOUSE_BUTTON_0, Input.GetMouseButton(0));
        inputPlayer.buttons.Set(NetworkInputPlayer.MOUSE_BUTTON_1, Input.GetMouseButton(1));
        inputPlayer.buttons.Set(NetworkInputPlayer.JUMP, Input.GetButton("Jump"));
        inputPlayer.buttons.Set(NetworkInputPlayer.DASH, Input.GetKey(KeyCode.LeftAlt));
        inputPlayer.buttons.Set(NetworkInputPlayer.CROUCH, Input.GetKey(KeyCode.LeftControl));
        inputPlayer.buttons.Set(NetworkInputPlayer.RUN, Input.GetKey(KeyCode.LeftShift));

        // 3. Dirección de la mirada (Mouse)
        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, 0.5f, 0f));
            if (plane.Raycast(ray, out float enter))
            {
                inputPlayer.lookDirection = ray.GetPoint(enter);
            }
        }

        input.Set(inputPlayer);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
