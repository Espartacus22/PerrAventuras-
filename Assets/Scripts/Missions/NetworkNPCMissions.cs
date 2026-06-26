using UnityEngine;
using Fusion;
using System.Collections;

public class NetworkNPCMissions : NetworkBehaviour, IInteractable
{
    public enum MissionMode { ForjaCollar, RecompensaSalto }

    [Header("Configuración General")]
    [SerializeField] private MissionMode modoNPC;

    [Header("Configuración Forja")]
    [SerializeField] private MissionData missionToGive;
    [SerializeField] private int gemaItemId = 0;
    [SerializeField] private int tiraItemId = 1;
    [SerializeField] private int collarResultId = 2;

    [Header("Configuración Salto (Monedas)")]
    [SerializeField] private int cantidadMonedas = 3;

    [Header("UI Local")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMPro.TMP_Text dialogueText;

    public void Interact() { }

    public void ProcessNPCInteraction(NetworkObject playerNetObj)
    {
        // El servidor siempre procesa la lógica principal
        if (!Object.HasStateAuthority) return;
        if (playerNetObj == null) return;

        if (modoNPC == MissionMode.ForjaCollar)
        {
            MissionController missionController = playerNetObj.GetComponent<MissionController>();
            InventorySystem inventory = playerNetObj.GetComponent<InventorySystem>();
            if (missionController == null || inventory == null) return;

            if (missionController.CurrentMission == null)
            {
                missionController.StartNewMission(missionToGive);
                RPC_ShowDialogueClient("¡Hola! Traeme 1 Gema y 1 Base Collar para forjar tu recompensa.", playerNetObj.InputAuthority);
                return;
            }

            if (inventory.HasItem(gemaItemId) && inventory.HasItem(tiraItemId))
            {
                inventory.RemoveItem(gemaItemId);
                inventory.RemoveItem(tiraItemId);
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
        else if (modoNPC == MissionMode.RecompensaSalto)
        {
            PlayerLevel playerLevel = playerNetObj.GetComponent<PlayerLevel>();
            PlayerMovement movement = playerNetObj.GetComponent<PlayerMovement>();

            if (playerLevel != null)
            {
                if (movement != null && movement.HasDoubleJump())
                {
                    RPC_ShowDialogueClient("¡Ya eres un maestro del doble salto!", playerNetObj.InputAuthority);
                }
                else if (playerLevel.currentCoins >= cantidadMonedas)
                {
                    // Intentamos gastar las monedas usando el método sincronizado de PlayerLevel
                    if (playerLevel.SpendCoins(cantidadMonedas))
                    {
                        if (movement != null) movement.UnlockDoubleJump();
                        RPC_ShowDialogueClient("¡Excelente! Has entregado las monedas, ahora puedes realizar un doble salto.", playerNetObj.InputAuthority);
                    }
                }
                else
                {
                    RPC_ShowDialogueClient("¡Hola viajero! Si me consigues 3 monedas, te enseñaré el arte del doble salto.", playerNetObj.InputAuthority);
                }
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowDialogueClient(string message, [RpcTarget] PlayerRef target)
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = message;

            // Apaga cualquier contador anterior y arranca uno nuevo para evitar superposiciones
            StopAllCoroutines();
            StartCoroutine(HideDialogueRoutine());
        }
    }

    private IEnumerator HideDialogueRoutine()
    {
        yield return new WaitForSeconds(4f);
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
}