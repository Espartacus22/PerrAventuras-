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

    // Nuestro único Prefab que contiene ambos modelos
    [SerializeField] private NetworkObject _playerPrefab;

    [Header("Puntos de Aparición (Spawns)")]
    // Arrastra aquí tus Empty GameObjects desde la escena (Spawn 1, Spawn 2...)
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Pickups (spawneados por server al crear sala)")]
    [SerializeField] private NetworkObject[] _pickupPrefabs;
    [SerializeField] private int _pickupAmount = 25;
    [SerializeField] private float _pickupRadius = 12f;

    private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();
    private bool _pickupsSpawned = false;

    private void Start()
    {
        _createRoomButton.onClick.AddListener(CreateRoom);
        _joinRoomButton.onClick.AddListener(JoinRoom);
    }

    private void Update()
    {
        // Los inputs del mouse ahora se manejan directamente en el OnInput
    }

    private async void CreateRoom()
    {
        Debug.Log("[NETWORK] Creando sala como Host...");
        try
        {
            var gameArg = new StartGameArgs()
            {
                GameMode = GameMode.Host,
                SessionName = "Room_01",
                SceneManager = _networkSceneManagerDefault,
                Scene = SceneRef.FromIndex(3) // Asegúrate de que este sea el índice correcto de tu escena
            };

            var result = await _networkRunner.StartGame(gameArg);

            if (!result.Ok)
            {
                Debug.LogError($"[NETWORK ERROR] Falló al crear la sala: {result.ShutdownReason}");
                return;
            }

            Debug.Log("[NETWORK] ¡SALA CREADA CON ÉXITO!");

            if (_networkRunner.IsServer && !_pickupsSpawned)
            {
                _pickupsSpawned = true;
                if (_pickupPrefabs != null && _pickupPrefabs.Length > 0)
                {
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
        // El spawn de pickups ya se maneja en el CreateRoom, pero se deja por seguridad
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"OnPlayerJoined: Player {player.PlayerId} se ha unido.");

        if (_lobbyPanel != null) _lobbyPanel.SetActive(false);

        if (!runner.IsServer) return;

        int spawnIndex = _players.Count;

        // 1. Calculamos la posición base
        Transform spawnPoint = (_spawnPoints != null && _spawnPoints.Length > 0) ? _spawnPoints[0] : null;
        Vector3 basePos = spawnPoint != null ? spawnPoint.position : Vector3.up * 3f;
        Quaternion baseRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        // 2. SPAWN
        var playerSpawned = runner.Spawn(_playerPrefab, basePos, baseRot, player, (runner, obj) =>
        {
            // --- AQUÍ ESTÁ EL TRUCO ---
            // Calculamos el desplazamiento
            float offset = spawnIndex * 2f;
            Vector3 finalPos = basePos + (spawnPoint != null ? spawnPoint.right : Vector3.right) * offset;

            // Intentamos moverlo por NCC si existe, sino por transform normal
            var ncc = obj.GetComponent<NetworkCharacterController>();
            if (ncc != null)
            {
                ncc.Teleport(finalPos); // Esto le avisa a la física de Fusion que debe moverse
            }
            else
            {
                obj.transform.position = finalPos;
            }

            // Configuramos el modelo visual
            var visuals = obj.GetComponent<PlayerVisuals>();
            if (visuals != null) visuals.ModelIndex = spawnIndex;
        });

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

        Vector2 rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 finalMove = Vector2.zero;

        // Calculamos la dirección respecto a la cámara local del jugador
        if (Camera.main != null && rawInput.sqrMagnitude > 0.01f)
        {
            Transform cam = Camera.main.transform;
            Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;

            Vector3 calculatedDir = (camForward * rawInput.y + camRight * rawInput.x).normalized;
            finalMove = new Vector2(calculatedDir.x, calculatedDir.z);
        }

        inputPlayer.moveInput = finalMove;

        // Botones
        inputPlayer.buttons.Set(NetworkInputPlayer.MOUSE_BUTTON_0, Input.GetMouseButton(0));
        inputPlayer.buttons.Set(NetworkInputPlayer.MOUSE_BUTTON_1, Input.GetMouseButton(1));
        inputPlayer.buttons.Set(NetworkInputPlayer.JUMP, Input.GetButton("Jump"));
        inputPlayer.buttons.Set(NetworkInputPlayer.DASH, Input.GetKey(KeyCode.LeftAlt));
        inputPlayer.buttons.Set(NetworkInputPlayer.CROUCH, Input.GetKey(KeyCode.LeftControl));
        inputPlayer.buttons.Set(NetworkInputPlayer.RUN, Input.GetKey(KeyCode.LeftShift));

        // Puntero del mouse
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

    // --- MÉTODOS REQUERIDOS POR LA INTERFAZ ---
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