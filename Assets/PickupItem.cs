using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Datos del item")]
    public string itemName = "Mitad hueso";
    public int quantity = 1;

    private bool playerNearby = false;

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.F))
        {
            PickUp();
        }
    }

    void PickUp()
    {
        Debug.Log($"Recogiste {quantity} {itemName}");
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            InteractionUI.Show($"Pulsa [F] para recoger {itemName}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            InteractionUI.Hide();
        }
    }
}
