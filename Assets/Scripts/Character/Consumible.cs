using UnityEngine;

public class Consumible : MonoBehaviour

{
    // Atributos base para cualquier ítem
    public string itemName = "New Item";
    public Sprite icon = null;
    public string description = "";
}

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
public class Consumable : Item
{
    // Atributo específico para consumibles de curación
    public int healingAmount = 20; // Cantidad de vida restaurada (valor predeterminado, se puede editar en el inspector)

    // Método para consumir el objeto y aplicar su efecto
    public void Consume()
    {
        // Asumimos que hay un componente PlayerHealth en el jugador para aplicar la curación.
        // En un sistema real, esto se integraría con el inventario y el jugador.
        PlayerHealth playerHealth = GameObject.FindWithTag("Player")?.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.Heal(healingAmount);
            Debug.Log($"Consumible usado: {itemName}. Vida restaurada: {healingAmount}");
        }
        else
        {
            Debug.LogWarning("No se encontró el componente PlayerHealth en el jugador.");
        }
    }
}

// Ejemplo de clase PlayerHealth (para completar el sistema, se debe implementar en el jugador)
public class PlayerHealth : MonoBehaviour
{
    public int currentHealth = 100;
    public int maxHealth = 100;

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Vida actual: {currentHealth}/{maxHealth}");
    }
}
