using UnityEngine;

public class ShadowFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float rayDistance = 50f;
    [SerializeField] private float yOffset = 0.02f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 origin = target.position + Vector3.up * 2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, groundMask))
        {
            transform.position = hit.point + Vector3.up * yOffset;
        }
    }
}
