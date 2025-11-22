using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryUI.I.UseSlot(slotIndex); // Consumir
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            InventoryUI.I.RightClickSlot(slotIndex); // Descartar (dropear)
        }
    }
}