using UnityEngine;

public class ShadowFollower : MonoBehaviour
{
    public Transform player;        // arrastrá el player (Piki)
    public float heightOffset = 0.05f; // cuán cerca del suelo está
    public LayerMask groundMask;    // asigná tu capa "Ground" o "Default"

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
