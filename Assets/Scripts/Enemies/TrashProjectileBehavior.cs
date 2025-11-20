using UnityEngine;

public class TrashProjectileBehavior : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 5f;
    public int damage = 10;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLevel lvl = other.GetComponent<PlayerLevel>();
            if (lvl != null)
                lvl.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // choca con el mundo o algo más -> destruir
        if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
