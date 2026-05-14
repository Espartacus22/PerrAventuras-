using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Networking;
using UnityEngine.SceneManagement;

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

    [Header("Spawns & Pickups")]
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private NetworkObject[] _pickupPrefabs;
    [SerializeField] private int _pickupAmount = 25;
    [SerializeField] private float _pickupRadius = 12f;

    private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();
    private bool _pickupsSpawned = false;

    private void Awake()
    {
        // Auto-asignación por si se pierden las referencias en el Inspector
        if (_networkRunner == null) _networkRunner = GetComponent<NetworkRunner>();
        if (_networkSceneManagerDefault == null) _networkSceneManagerDefault = GetComponent<NetworkSceneManagerDefault>();

        if (_networkRunner != null) _networkRunner.ProvideInput = true;
    }

    private void Start()
    {
        if (_createRoomButton != null) _createRoomButton.onClick.AddListener(CreateRoom);
        if (_joinRoomButton != null) _joinRoomButton.onClick.AddListener(JoinRoom);
    }

    // Método para limpiar todo y resetear el menú
    private async void ResetLobby()
    {
        if (_networkRunner != null) await _networkRunner.Shutdown();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SetButtonsInteractable(bool state)
    {
        if (_createRoomButton != null) _createRoomButton.interactable = state;
        if (_joinRoomButton != null) _joinRoomButton.interactable = state;
    }

    private async void CreateRoom()
    {
        SetButtonsInteractable(false);
        try
        {
            var result = await _networkRunner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Host,
                SessionName = "Room_01",
                SceneManager = _networkSceneManagerDefault,
                Scene = SceneRef.FromIndex(3)
            });

            if (!result.Ok) ResetLobby();
        }
        catch (Exception e)
        {
            Debug.LogError($"[CREATE ERROR] {e.Message}");
            ResetLobby();
        }
    }

    private async void JoinRoom()
    {
        SetButtonsInteractable(false);
        try
        {
            var result = await _networkRunner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SessionName = "Room_01",
                SceneManager = _networkSceneManagerDefault,
                Scene = SceneRef.FromIndex(3)
            });

            if (!result.Ok)
            {
                // Si la sala no existe o falla, reseteamos para liberar botones
                Debug.LogWarning($"[JOIN ERROR] {result.ShutdownReason}");
                ResetLobby();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[JOIN CRASH] {e.Message}");
            ResetLobby();
        }
    }

    // --- ONINPUT: TU LÓGICA DE MELEE ORIGINAL (CON GUIONES BAJOS) ---
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var inputPlayer = new NetworkInputPlayer();
        Vector2 rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Camera.main != null && rawInput.sqrMagnitude > 0.01f)
        {
            Transform cam = Camera.main.transform;
            Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;
            Vector3 calculatedDir = (camForward * rawInput.y + camRight * rawInput.x).normalized;
            inputPlayer.moveInput = new Vector2(calculatedDir.x, calculatedDir.z);
        }

        // Botones recuperados
        inputPlayer.buttons.Set(NetworkInputPlayer.MOUSE_BUTTON_0, Input.GetMouseButton(0));
        inputPlayer.buttons.Set(NetworkInputPlayer.MOUSE_BUTTON_1, Input.GetMouseButton(1));
        inputPlayer.buttons.Set(NetworkInputPlayer.JUMP, Input.GetButton("Jump"));
        inputPlayer.buttons.Set(NetworkInputPlayer.DASH, Input.GetKey(KeyCode.LeftAlt));
        inputPlayer.buttons.Set(NetworkInputPlayer.CROUCH, Input.GetKey(KeyCode.LeftControl));
        inputPlayer.buttons.Set(NetworkInputPlayer.RUN, Input.GetKey(KeyCode.LeftShift));

        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, 0.5f, 0f));
            if (plane.Raycast(ray, out float enter))
                inputPlayer.lookDirection = ray.GetPoint(enter);
        }
        input.Set(inputPlayer);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (_lobbyPanel != null) _lobbyPanel.SetActive(false);
        if (!runner.IsServer) return;

        int spawnIndex = Mathf.Max(0, player.PlayerId - 1);
        Vector3 basePos = Vector3.up * 3f + (Vector3.right * spawnIndex * 2f);

        runner.Spawn(_playerPrefab, basePos, Quaternion.identity, player, (runner, obj) => {
            var visuals = obj.GetComponent<PlayerVisuals>();
            if (visuals != null) visuals.ModelIndex = spawnIndex;
        });
    }

    // --- OTROS CALLBACKS ---
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
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
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}