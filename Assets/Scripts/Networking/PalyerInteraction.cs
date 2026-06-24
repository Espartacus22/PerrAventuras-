using UnityEngine;
using Fusion;

[DisallowMultipleComponent]
public class PlayerInteraction : NetworkBehaviour
{
    [Header("Configuración de Interacción")]
    [SerializeField] private float interactionRadius = 3f; // Distancia para presionar la E
    [SerializeField] private LayerMask interactableLayer;   // Capa 'Interactable'

    private void Update()
    {
        // BLINDAJE NETWORKING: Solo el jugador dueño de este personaje (local) puede presionar teclas
        if (!HasInputAuthority) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Tiramos una esfera invisible en el espacio para detectar si hay un NPC cerca
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius, interactableLayer);

            foreach (var col in colliders)
            {
                NetworkObject no = col.GetComponent<NetworkObject>();
                IInteractable interactable = col.GetComponent<IInteractable>();

                if (no != null && interactable != null)
                {
                    // Le enviamos el RPC al servidor pasándole el NetworkObject del NPC
                    RPC_Interact(no);
                    break; // Salimos del bucle para no hablar con dos cosas a la vez
                }
            }
        }
    }

    // 
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Interact(NetworkObject interactableObject)
    {
        // Autoridad del servidor obligatoria
        if (!Object.HasStateAuthority) return;
        if (interactableObject == null) return;

        // El servidor obtiene la interfaz del objeto interactuable (el NPC)
        IInteractable interactable = interactableObject.GetComponent<IInteractable>();

        if (interactable != null)
        {
            interactable.Interact(); // Ejecuta el método base

            // Conectamos con sistema de misiones y crafteo
            NetworkNPCMissions npcMissions = interactableObject.GetComponent<NetworkNPCMissions>();
            if (npcMissions != null)
            {
                // Le pasamos el 'this.Object' que es el NetworkObject de ESTE jugador
                npcMissions.ProcessNPCInteraction(this.Object);
            }
        }
    }

    // Dibuja el radio en el editor para ver el alcance
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}