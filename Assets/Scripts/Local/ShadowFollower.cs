using UnityEngine;

public class ShadowFollower : MonoBehaviour
{
    public Transform player;        // arrastr� el player (Piki)
    public float heightOffset = 0.05f; // cu�n cerca del suelo est�
    public LayerMask groundMask;    // asign� tu capa "Ground" o "Default"

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 origin = player.position + Vector3.up * 2f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 10f, groundMask))
        {
            transform.position = new Vector3(player.position.x, hit.point.y + heightOffset, player.position.z);
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }
}
