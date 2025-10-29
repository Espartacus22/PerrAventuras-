using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public float speed = 25f;
    public float lifetime = 5f;
    public string targetTag = "Enemy";

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
            Debug.Log("Impacto con " + other.name);
        }

        Destroy(gameObject);
    }
}

