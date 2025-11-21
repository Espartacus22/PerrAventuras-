using UnityEngine;

public class ChestInteraction : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject itemPrefab;      // El prefab del ítem (gema, moneda, etc.)
    public Transform dropPoint;        // El punto donde cae el ítem
    public Material openedMaterial;    // Material cuando ya está abierto (opcional)

    private bool isOpened = false;
    private bool playerNearby = false;
    private MeshRenderer rend;

    private void Start()
    {
        rend = GetComponentInChildren<MeshRenderer>();
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !isOpened)
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpened = true;

        // Cambiar apariencia (opcional)
        if (openedMaterial && rend) rend.material = openedMaterial;

        // Spawnear el ítem con física
        GameObject item = Instantiate(itemPrefab, dropPoint.position, Quaternion.identity);
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(Random.insideUnitSphere * 2f + Vector3.up * 4f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            InteractionUI.Show("Pulsa [E] para abrir el cofre");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            InteractionUI.Hide();
        }
    }
}
}
