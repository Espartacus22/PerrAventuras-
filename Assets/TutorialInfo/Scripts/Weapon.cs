using UnityEngine;

public class Weapon : MonoBehaviour

{
    public Sword sword;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.EquiparArma(sword);
                Destroy(gameObject);
            }
        }
    }
}
