using UnityEngine;
using Fusion;
using System.Collections; // Necesario para la Corrutina de tiempo

public class NetworkNPCMissions : NetworkBehaviour, IInteractable
{
    [Header("Configuración de Misión")]
    [SerializeField] private MissionData missionToGive;
    [SerializeField] private int gemaItemId = 0;
    [SerializeField] private int tiraItemId = 1;
    [SerializeField] private int collarResultId = 2;

    [Header("UI Local (Solo Clientes)")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMPro.TMP_Text dialogueText;

    public void Interact() { }

    public void ProcessNPCInteraction(NetworkObject playerNetObj)
    {
        if (!Object.HasStateAuthority) return;
        if (playerNetObj == null) return;

        MissionController missionController = playerNetObj.GetComponent<MissionController>();
        InventorySystem inventory = playerNetObj.GetComponent<InventorySystem>();

        if (missionController == null || inventory == null) return;

        // FASE 1: Entregar misión si no la tiene
        if (missionController.CurrentMission == null)
        {
            missionController.StartNewMission(missionToGive);
            RPC_ShowDialogueClient("¡Hola! Traeme 1 Gema y 1 Base Collar para forjar tu recompensa.", playerNetObj.InputAuthority);
            return;
        }

        // FASE 3: Crafteo validando inventario físico (Ignoramos la fase 2 para evitar el bug lógico)
        if (inventory.HasItem(gemaItemId) && inventory.HasItem(tiraItemId))
        {
            // El servidor borra los materiales físicos
            inventory.RemoveItem(gemaItemId);
            inventory.RemoveItem(tiraItemId);

            // El servidor agrega el collar final
            if (inventory.AddItem(collarResultId))
            {
                RPC_ShowDialogueClient("¡Excelente! Aquí tienes tu Collar forjado de forma segura.", playerNetObj.InputAuthority);
                missionController.UpdateProgress("Talk_NPC_Forge", 1);
            }
            else
            {
                RPC_ShowDialogueClient("Libera espacio en tu inventario para poder darte el collar.", playerNetObj.InputAuthority);
            }
        }
        else
        {
            RPC_ShowDialogueClient("Aún te faltan materiales. Busca la Gema y la Base Collar.", playerNetObj.InputAuthority);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowDialogueClient(string message, [RpcTarget] PlayerRef target)
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = message;

            // Apaga cualquier contador anterior y arranca uno nuevo
            StopAllCoroutines();
            StartCoroutine(HideDialogueRoutine());
        }
    }

    // Rutina que espera 4 segundos y apaga el panel
    private IEnumerator HideDialogueRoutine()
    {
        yield return new WaitForSeconds(4f);
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
}