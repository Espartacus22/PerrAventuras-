using UnityEngine;

public class EndDoorOpener : MonoBehaviour
{
    [Header("Puerta a abrir")]
    [Tooltip("Objeto que bloquea la salida. Se desactiva cuando el player entra en la zona.")]
    public GameObject doorBlocker;

    [Header("Solo se puede abrir una vez")]
    public bool oneShot = true;

    private bool _opened = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneShot && _opened) return;

        _opened = true;

        if (doorBlocker != null)
        {
            doorBlocker.SetActive(false);   // puerta desaparece
        }

        Debug.Log("EndDoorOpener: puerta final abierta (llegó a la zona del árbol).");
    }
}
