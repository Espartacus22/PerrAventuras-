using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

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
            players[i].SetActive(i == currentIndex);
        }

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
        // Desactivar actual
        players[currentIndex].SetActive(false);

        // Avanzar al siguiente
        currentIndex = (currentIndex + 1) % players.Count;

        // Activar nuevo
        players[currentIndex].SetActive(true);

        // Habilitar solo el componente PlayerLocal del activo
        SetPlayerLocalEnabled(players[currentIndex]);

        // Actualizar cámara
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
            Component cineComp = cinemachineCameraObject.GetComponent("CinemachineCamera") as Component;
            if (cineComp != null)
            {
                try
                {
                    // obtener propiedad "Target"
                    var targetProp = cineComp.GetType().GetProperty("Target", BindingFlags.Public | BindingFlags.Instance);
                    if (targetProp != null)
                    {
                        var targetObj = targetProp.GetValue(cineComp, null);
                        if (targetObj != null)
                        {
                            // intentar asignar TrackingTarget
                            var trackingProp = targetObj.GetType().GetProperty("TrackingTarget", BindingFlags.Public | BindingFlags.Instance);
                            if (trackingProp != null && trackingProp.CanWrite)
                            {
                                trackingProp.SetValue(targetObj, targetTransform, null);
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("CharacterSwitchManager: error al intentar asignar target a CinemachineCamera via reflexión: " + ex.Message);
                }
            }
        }

        // 3) Fallback: parentear la mainCamera al personaje
        if (mainCamera != null && targetTransform != null)
        {
            mainCamera.transform.SetParent(targetTransform);
            // Posición y rotación local por defecto — ajustá los valores
            mainCamera.transform.localPosition = new Vector3(0f, 2.5f, -4f);
            mainCamera.transform.localEulerAngles = Vector3.zero;
        }
    }
}
