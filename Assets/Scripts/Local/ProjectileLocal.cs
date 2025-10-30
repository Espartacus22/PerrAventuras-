using UnityEngine;

public class ProjectileLocal : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootForce = 25f;
    public float shootCooldown = 0.4f;
    private float lastShootTime = -99f;

    public void Shoot()
    {
        if (Time.time - lastShootTime < shootCooldown) return;
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("ProjectileLocal: missing prefab or firePoint on " + gameObject.name);
            return;
        }

        Debug.Log("ProjectileLocal: Shooting from " + gameObject.name);
        GameObject p = GameObject.Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = p.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = firePoint.forward * shootForce;
        lastShootTime = Time.time;
    }
}
