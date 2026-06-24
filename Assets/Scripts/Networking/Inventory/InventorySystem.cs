using Fusion;
using UnityEngine;

public class InventorySystem : NetworkBehaviour
{
    const int SLOTS = 6;
    const float PICKUP_RANGE = 4f;

    [Networked, Capacity(SLOTS), OnChangedRender(nameof(OnInventoryChanged))]
    private NetworkArray<int> Items => default;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            for (int i = 0; i < SLOTS; i++) Items.Set(i, -1);
        }

        if (HasInputAuthority && InventoryUI.I != null)
        {
            InventoryUI.I.AttachInventory(this);
            RefreshUI();
        }
    }

    public void TryPickup(NetworkObject pickupObj)
    {
        if (!Object.HasStateAuthority) return;

        var pickup = pickupObj.GetComponent<NetworkPickup>();
        if (pickup == null) return;

        if (Vector3.Distance(transform.position, pickupObj.transform.position) > PICKUP_RANGE) return;

        int freeSlot = FindFreeSlot();
        if (freeSlot != -1)
        {
            Items.Set(freeSlot, pickup.ItemId);
            pickup.ServerCollect();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UseItem(int slot)
    {
        if (!Object.HasStateAuthority || slot < 0 || slot >= SLOTS || Items[slot] == -1) return;

        var item = ItemDatabase.GetItem(Items[slot]);

        // 1. LÓGICA PARA CONSUMIBLES (Pociones, Comida)
        if (item?.isConsumable == true)
        {
            PlayerLevel playerStats = GetComponent<PlayerLevel>();

            if (playerStats != null)
            {
                // Solo entramos si la vida actual es menor a la máxima
                if (playerStats.currentHealth < playerStats.maxHealth)
                {
                    playerStats.Heal((int)item.healAmount);
                    Debug.Log($"[INVENTARIO] Usaste {item.itemName}. Vida restaurada.");

                    // Solo si se usó efectivamente, vaciamos el slot
                    Items.Set(slot, -1);
                }
                else
                {
                    Debug.Log("[INVENTARIO] Vida completa, poción reservada.");
                }
            }
        }
        // 2. NUEVA LÓGICA PARA EQUIPABLES (Collares, Armaduras)
        else if (item?.isEquipable == true)
        {
            Debug.Log($"[INVENTARIO] Te equipaste: {item.itemName}.");

            
     
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_DiscardItem(int slot)
    {
        if (!Object.HasStateAuthority || slot < 0 || slot >= SLOTS || Items[slot] == -1) return;

        int itemId = Items[slot];
        Items.Set(slot, -1);

        var itemData = ItemDatabase.GetItem(itemId);

        if (itemData != null && itemData.pickupPrefab != null)
        {
            Runner.Spawn(itemData.pickupPrefab.GetComponent<NetworkObject>(),
                transform.position + transform.forward * 1.5f,
                Quaternion.identity);
        }
    }

    private int FindFreeSlot()
    {
        for (int i = 0; i < SLOTS; i++) { if (Items[i] == -1) return i; }
        return -1;
    }

    private void RefreshUI()
    {
        if (Object.HasInputAuthority) InventoryUI.I?.Refresh(Items.ToArray());
    }

    private void OnInventoryChanged() => RefreshUI();

    // --- MÉTODOS PÚBLICOS PARA EL NPC Y MISIONES ---

    public bool HasItem(int itemId)
    {
        for (int i = 0; i < SLOTS; i++)
        {
            if (Items[i] == itemId) return true;
        }
        return false;
    }

    public bool RemoveItem(int itemId)
    {
        if (!Object.HasStateAuthority) return false;

        for (int i = 0; i < SLOTS; i++)
        {
            if (Items[i] == itemId)
            {
                Items.Set(i, -1);
                return true;
            }
        }
        return false;
    }

    public bool AddItem(int itemId)
    {
        if (!Object.HasStateAuthority) return false;

        int freeSlot = FindFreeSlot();
        if (freeSlot != -1)
        {
            Items.Set(freeSlot, itemId);
            return true;
        }
        return false;
    }
}