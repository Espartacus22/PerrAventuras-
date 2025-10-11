using UnityEngine;

public class XPOrb : MonoBehaviour
{
    public int xpAmount = 10;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLevel playerLevel = other.GetComponent<PlayerLevel>();
            if (playerLevel != null)
                playerLevel.GainXP(xpAmount);

            Destroy(gameObject);
        }
    }
}
