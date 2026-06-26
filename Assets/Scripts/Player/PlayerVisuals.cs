
using Fusion;
using UnityEngine;

public class PlayerVisuals : NetworkBehaviour
{
    [Header("Modelos 3D")]
    [SerializeField] private GameObject _modelCharacterA; // Beagle
    [SerializeField] private GameObject _modelCharacterB; // Caniche

    // El Networked permite que todos los clientes vean qué modelo está activo
    [Networked, OnChangedRender(nameof(UpdateVisuals))]
    public int ModelIndex { get; set; }

    public override void Spawned()
    {
        // Forzamos la actualización inicial al aparecer
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        // Debug para saber qué está haciendo cada jugador
        Debug.Log($"[{gameObject.name}] Actualizando visuales. Index actual: {ModelIndex}");

        if (_modelCharacterA == null || _modelCharacterB == null)
        {
            Debug.LogError("Faltan referencias a los modelos en PlayerVisuals");
            return;
        }

        // Definimos cuál modelo activar
        bool isBeagle = (ModelIndex == 1);

        GameObject activeModel = isBeagle ? _modelCharacterA : _modelCharacterB;
        Animator newAnimator = activeModel.GetComponentInChildren<Animator>();

        try
        {
            // Activamos/Desactivamos
            _modelCharacterA.SetActive(isBeagle);
            _modelCharacterB.SetActive(!isBeagle);

            if (newAnimator != null && newAnimator.runtimeAnimatorController != null)
            {
                var netMecanim = GetComponent<NetworkMecanimAnimator>();

                if (netMecanim != null)
                {
                    // Deshabilitar temporalmente evita conflictos de sincronización
                    netMecanim.enabled = false;

                    // Asignamos el nuevo Animator
                    netMecanim.Animator = newAnimator;

                    // --- RESET FORZADO ---
                    // Esto es vital para que el Animator pase de "congelado" a "activo"
                    newAnimator.Rebind();
                    newAnimator.Update(0);
                    // ---------------------

                    netMecanim.enabled = true;
                    Debug.Log($"[{gameObject.name}] Animator asignado correctamente a: {newAnimator.name}");
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] El modelo activo no tiene un Animator válido.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error crítico en UpdateVisuals: " + e.Message);
        }
    }

}

