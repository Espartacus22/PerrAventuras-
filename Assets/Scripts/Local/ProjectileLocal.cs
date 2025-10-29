using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ProjectileLocal : MonoBehaviour
{
    [Header("Configuración de disparo")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootCooldown = 0.5f;
    private float lastShootTime;

    public void Shoot()
    {
        if (Time.time - lastShootTime < shootCooldown)
        {
            Debug.Log("Cooldown activo, no puede disparar todavía.");
            return;
        }

        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("ProjectileLocal: Falta prefab o firePoint en " + gameObject.name);
            return;
        }

        Debug.Log("Disparando proyectil desde " + gameObject.name + " en posición: " + firePoint.position);

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        if (proj == null)
        {
            Debug.LogError("El proyectil no se instanció correctamente!");
        }
        else
        {
            Debug.Log("Proyectil instanciado correctamente: " + proj.name);
        }

        lastShootTime = Time.time;
    }
}
