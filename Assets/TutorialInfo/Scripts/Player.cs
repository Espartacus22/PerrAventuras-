using UnityEngine;

public class Player : MonoBehaviour
{
    public Sword swordequipped; // Referencia al arma actualmente equipada
    public Transform equipped; // Punto donde el arma será "sostenida" (por ejemplo, la mano del personaje)
    public ScriptableObject espada;
    public void EquiparArma(Sword nuevaArma)
    {
        // Si ya hay un arma equipada, podrías destruirla o desactivarla
        if (espada != null)
        {
            // Opcional: Destruir el objeto del arma anterior o manejarlo según tu lógica
            // Destroy(armaEquipada.gameObject);
        }

        // Asignar la nueva arma
        espada = nuevaArma;

        // Instanciar el modelo visual del arma en el punto de sujeción (si lo tienes)
        if (nuevaArma.modelo != null && equipped != null)
        {
            GameObject armaInstanciada = Instantiate(nuevaArma.modelo, equipped.position, equipped.rotation, equipped);
            armaInstanciada.name = nuevaArma.nombre; // Para identificarla en la jerarquía
        }

        Debug.Log($"Arma equipada: {nuevaArma.nombre}");
    }

    // Ejemplo de uso del arma (puedes expandir esta lógica)
    private void Update()
    {
        if (swordequipped != null && Input.GetKeyDown(KeyCode.Space)) // Por ejemplo, disparar con la tecla Espacio
        {
            Debug.Log($"Disparando con {swordequipped.nombre}, Daño: {swordequipped.dano}");
            // Aquí puedes añadir la lógica de disparo, animaciones, etc.
        }
    }
}
