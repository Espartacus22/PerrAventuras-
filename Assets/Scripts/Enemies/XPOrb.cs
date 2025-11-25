using UnityEngine;

public class XPOrb : MonoBehaviour
{
    public int xpAmount = 10;
    public float attractRadius = 5f;
    public float attractSpeed = 8f;

    Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < attractRadius)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * attractSpeed * Time.deltaTime;
        }
    }

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
