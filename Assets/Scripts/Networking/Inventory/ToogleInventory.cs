using UnityEngine;

public class ToggleInventory : MonoBehaviour
{
    [SerializeField] GameObject inventoryPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }
}