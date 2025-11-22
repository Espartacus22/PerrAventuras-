using Fusion;
using UnityEngine;

public class InventorySystem : NetworkBehaviour
{
    const int SLOTS = 6; // cantidad de slots de inventario disponibles de manera incial (expandibles durante gameplay)
    const float PICKUP_RANGE = 4f;

    [Networked, Capacity(SLOTS), OnChangedRender(nameof(OnInventoryChanged))]
    private NetworkArray<int> Items => default;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            for (int i = 0; i < SLOTS; i++)
                Items.Set(i, -1);
        }
    }

    // Pickup directo (server)
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
            RefreshUI();
        }
    }

    // Consumir ítem
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UseItem(int slot)
    {
        if (!Object.HasStateAuthority || slot < 0 || slot >= SLOTS || Items[slot] == -1) return;

        var item = ItemDatabase.GetItem(Items[slot]);
        if (item?.isConsumable == true)
        {
            GetComponent<PlayerHealth>()?.Heal((int)item.healAmount);
            Items.Set(slot, -1);
        }

        RefreshUI();
    }

    // Descartar ítem (dropear en el mundo)
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

        RefreshUI();
    }

    private int FindFreeSlot()
    {
        for (int i = 0; i < SLOTS; i++)
            if (Items[i] == -1) return i;
        return -1;
    }

    private void RefreshUI()
    {
        if (Object.HasInputAuthority)
            InventoryUI.I?.Refresh(Items.ToArray());
    }

    private void OnInventoryChanged()
    {
        if (Object.HasInputAuthority)
            InventoryUI.I?.Refresh(Items.ToArray());
    }
}