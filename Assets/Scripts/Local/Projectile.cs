using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public int damage = 1;
    public string targetTag = "Enemy"; // por defecto apunta a enemigos

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            Debug.Log($"{name} impactó contra {targetTag}");
            Destroy(gameObject);
        }
    }

    // Permite definir dinámicamente a quién apunta (Player o Enemy)
    public void SetTargetTag(string newTag)
    {
        targetTag = newTag;
    }
}
