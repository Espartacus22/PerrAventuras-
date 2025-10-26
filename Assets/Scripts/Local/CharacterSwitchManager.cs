using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.Cinemachine;

public class CharacterSwitchManager : MonoBehaviour
{
    [Header("Personajes")]
    public GameObject characterA; // Piki (r�pida)
    public GameObject characterB; // Heavy (fuerte)
    public GameObject characterC; // Tornado

    [Header("C�mara (fallback si no us�s Cinemachine)")]
    public Camera mainCamera; // c�mara principal

    [Header("Cinemachine (opcional)")]
    // Arrastr� aqu� el GameObject que contiene la c�mara de Cinemachine
    public GameObject cinemachineCameraObject;

    [Header("Configuraci�n")]
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

        // Activar solo el primero y desactivar los dem�s
        for (int i = 0; i < players.Count; i++)
        {
            var pl = players[i].GetComponent<PlayerLocal>();
            if (pl != null) pl.enabled = (i == currentIndex);
            players[i].SetActive(true); // todos activos visualmente
        }

        // Intentar configurar la c�mara para el primer personaje
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

        // Actualizar c�mara
        UpdateCameraTarget(players[currentIndex].transform);
    }

    void SetPlayerLocalEnabled(GameObject active)
    {
        // Desactivar PlayerLocal en todos
        foreach (var p in players)
        {
            var pl = p.GetComponent<PlayerLocal>();
            if (pl != null) pl.enabled = false;
        }

        // Activar en el actual
        var activePl = active.GetComponent<PlayerLocal>();
        if (activePl != null) activePl.enabled = true;
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