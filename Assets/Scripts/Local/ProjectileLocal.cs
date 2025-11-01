using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ProjectileLocal : MonoBehaviour
{
    [Header("Prefab y punto de disparo")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Balística")]
    public float shootForce = 25f;
    public float shootCooldown = 0.4f;
    private float lastShootTime = -99f;

    [Header("Límite de proyectiles")]
    public int maxProjectiles = 5;                // cuántos proyectiles activos permitimos
    private List<GameObject> activeProjectiles = new List<GameObject>();

    [Header("Referencia opcional a PlayerLevel (para ajustar daño)")]
    public PlayerLevel playerLevelRef; // ASIGNA en inspector si querés escalar daño con nivel

    public void Shoot()
    {
        if (Time.time - lastShootTime < shootCooldown) return;

        // limpiar lista de referencias muertas
        activeProjectiles.RemoveAll(p => p == null);
        if (activeProjectiles.Count >= maxProjectiles)
        {
            Debug.Log("Límite de proyectiles activos alcanzado");
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning("ProjectileLocal: Falta projectilePrefab en " + gameObject.name);
            return;
        }

        // origen y dirección según cámara (más natural para shooter)
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 dir = cam != null ? cam.forward : (firePoint != null ? firePoint.forward : transform.forward);
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + dir * 0.5f;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = dir.normalized * shootForce;
        }

        // si el prefab tiene componente Projectile (o ProjectileBehaviour), intentar setear damage dinámico
        var projScript = proj.GetComponent<Projectile>();
        if (projScript != null && playerLevelRef != null)
        {
            // ejemplo: escala de daño por nivel, adaptá si tu PlayerLevel tiene otro nombre/propiedad
            projScript.damage = Mathf.RoundToInt(projScript.damage * (1f + playerLevelRef.damageMultiplier * playerLevelRef.currentLevel));
        }

        activeProjectiles.Add(proj);
        lastShootTime = Time.time;

        Debug.Log($"Disparo desde {gameObject.name} con fuerza {shootForce}");
        Debug.DrawRay(spawnPos, dir * 2f, Color.cyan, 1f);
    }
}
