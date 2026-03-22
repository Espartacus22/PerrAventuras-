using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HealthPickup : MonoBehaviour
{
    public int healAmount = 40;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerLevel lvl = other.GetComponent<PlayerLevel>();
        if (lvl != null)
        {
            lvl.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
