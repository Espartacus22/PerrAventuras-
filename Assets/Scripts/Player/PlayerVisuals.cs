using Fusion;
using UnityEngine;

public class PlayerVisuals : NetworkBehaviour
{
    [Header("Modelos 3D")]
    [SerializeField] private GameObject _modelCharacterA; // Arrastra tu modelo 1 aquí
    [SerializeField] private GameObject _modelCharacterB; // Arrastra tu modelo 2 aquí

    // Variable de red: 0 será para el Host, 1 será para el Cliente
    // OnChangedRender avisa a Unity que debe actualizar la vista si este número cambia
    [Networked, OnChangedRender(nameof(UpdateVisuals))]
    public int ModelIndex { get; set; }

    public override void Spawned()
    {
        // Apenas nace el jugador, actualizamos qué modelo debe verse
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (ModelIndex == 0)
        {
            _modelCharacterA.SetActive(true);
            _modelCharacterB.SetActive(false);
        }
        else
        {
            _modelCharacterA.SetActive(false);
            _modelCharacterB.SetActive(true);
        }
    }
}