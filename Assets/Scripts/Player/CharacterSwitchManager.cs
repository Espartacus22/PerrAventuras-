using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.Cinemachine;

public class CharacterSwitchManager : MonoBehaviour
{
    [Header("Personajes")]
    public GameObject characterA; // Piki (rápida)
    public GameObject characterB; // Heavy (fuerte)
    public GameObject characterC; // Tornado

    [Header("Cámara (fallback si no usás Cinemachine)")]
    public Camera mainCamera; // cámara principal

    [Header("Cinemachine (opcional)")]
    // Arrastrá aquí el GameObject que contiene la cámara de Cinemachine
    public GameObject cinemachineCameraObject;

    [Header("Configuración")]
    public KeyCode switchKey = KeyCode.Tab;

    private List<GameObject> players;
    private int currentIndex = 0;

    void Start()
    {
        // Construir lista ignorando posibles nulls
        players = new List<GameObject>();
        if (characterA != null) players.Add(characterA);
        if (characterB != null) players.Add(characterB);
        if (characterC != null) players.Add(characterC);

        if (players.Count == 0)
        {
            Debug.LogError("CharacterSwitchManager: No hay personajes asignados.");
            return;
        }

        // Activar solo el primero y desactivar los demás
        for (int i = 0; i < players.Count; i++)
        {
            var pm = players[i].GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = (i == currentIndex);
            players[i].SetActive(true);
        }

        ApplyControlState();

        // Intentar configurar la cámara para el primer personaje
        UpdateCameraTarget(players[currentIndex].transform);
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            Swap();
        }
    }

    void Swap()
    {

        // Avanzar al siguiente
        currentIndex = (currentIndex + 1) % players.Count;

        // Activar nuevo
        players[currentIndex].SetActive(true);

        // Habilitar solo el componente PlayerLocal del activo
        SetPlayerLocalEnabled(players[currentIndex]);

        ApplyControlState();
        // Actualizar cámara
        UpdateCameraTarget(players[currentIndex].transform);
    }

    void SetPlayerLocalEnabled(GameObject active)
    {
        // Desactivar PlayerMovement en todos
        foreach (var p in players)
        {
            var pm = p.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;
        }

        // Activar en el actual
        var activePm = active.GetComponent<PlayerMovement>();
        if (activePm != null) activePm.enabled = true;
    }

    void ApplyControlState()
    {
        for (int i = 0; i < players.Count; i++)
        {
            bool isActive = (i == currentIndex);
            SetPlayerState(players[i], isActive);
        }
    }

    void SetPlayerState(GameObject go, bool isActive)
    {
        // Control
        var pm = go.GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = isActive;

        // Follow/Companion
        var follow = go.GetComponent<FollowPlayer>();
        var agent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
        var rb = go.GetComponent<Rigidbody>();

        if (isActive)
        {
            if (follow != null) follow.enabled = false;
            if (agent != null) agent.enabled = false;
            if (rb != null) rb.isKinematic = false;
        }
        else
        {
            if (rb != null) rb.isKinematic = true;
            if (agent != null) agent.enabled = true;
            if (follow != null) follow.enabled = true;
        }
    }

    void UpdateCameraTarget(Transform targetTransform)
    {
        if (cinemachineCameraObject != null)
        {
            // Compatibilidad con Unity 6000 y nuevas APIs
            var cam = cinemachineCameraObject.GetComponent<CinemachineCamera>();
            var freeLook = cinemachineCameraObject.GetComponent<CinemachineFreeLook>();

            if (cam != null)
            {
                cam.Follow = targetTransform;
                cam.LookAt = targetTransform;
                return;
            }

            if (freeLook != null)
            {
                freeLook.Follow = targetTransform;
                freeLook.LookAt = targetTransform;
                return;
            }
        }

        // Fallback manual si no hay Cinemachine
        if (mainCamera != null)
        {
            mainCamera.transform.SetParent(targetTransform);
            mainCamera.transform.localPosition = new Vector3(0f, 2.5f, -4f);
            mainCamera.transform.localEulerAngles = Vector3.zero;
        }
    }
}