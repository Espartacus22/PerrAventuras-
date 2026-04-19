using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI I;
    [SerializeField] private Image[] slots = new Image[6]; // segun cantidad de slots de inventario 
    private InventorySystem _inventory;

    private void Awake() => I = this;

    public void AttachInventory(InventorySystem inventory)
    {
        _inventory = inventory;
    }

    public void Refresh(int[] itemIds)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < itemIds.Length && itemIds[i] != -1)
            {
                var item = ItemDatabase.GetItem(itemIds[i]);
                slots[i].sprite = item?.icon;
                slots[i].color = Color.white;

                var btn = slots[i].GetComponent<Button>() ?? slots[i].gameObject.AddComponent<Button>();
                int slotIndex = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => UseSlot(slotIndex));

                var trigger = slots[i].gameObject.GetComponent<InventorySlot>() ?? slots[i].gameObject.AddComponent<InventorySlot>();
                trigger.slotIndex = i;
            }
            else
            {
                slots[i].sprite = null;
                slots[i].color = new Color(0.15f, 0.15f, 0.15f);
            }
        }
    }

    public void UseSlot(int slot)
    {
        if (_inventory != null && _inventory.Object.HasInputAuthority)
        {
            _inventory.RPC_UseItem(slot);
        }
    }

    public void RightClickSlot(int slot)
    {
        if (_inventory != null && _inventory.Object.HasInputAuthority)
        {
            _inventory.RPC_DiscardItem(slot);
        }
    }

}