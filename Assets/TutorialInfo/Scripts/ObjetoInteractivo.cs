using UnityEngine;

public class ObjetoInteractivo : MonoBehaviour
{
    public Inventario inventario;
    void Start()
    {
        inventario = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventario>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            inventario.amount = inventario.amount + 1;
            Destroy(gameObject);
        }
    }
}
