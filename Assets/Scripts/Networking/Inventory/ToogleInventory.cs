using UnityEngine;

public class ToggleInventory : MonoBehaviour
{
    [SerializeField] GameObject inventoryPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isOpen = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isOpen);

            // GESTIÓN DEL MOUSE
            if (isOpen)
            {
                // Libera el mouse para poder usar el inventario
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Bloquea el mouse para volver a jugar/atacar
                //Cursor.lockState = CursorLockMode.Locked;
                //Cursor.visible = false;
            }
        }
    }
}