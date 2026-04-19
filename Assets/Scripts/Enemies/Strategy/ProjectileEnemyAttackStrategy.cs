using UnityEngine;
using Fusion; // ¡Añadido para usar la red!

public class ProjectileEnemyAttackStrategy : IEnemyAttackStrategy
{
    public void Execute(EnemyController enemy)
    {
        if (enemy.Target == null) return;
        if (enemy.ProjectilePrefab == null || enemy.FirePoint == null) return;

        // CANDADO DE RED: Solo el servidor está autorizado a disparar para que no se dupliquen balas
        if (!enemy.HasStateAuthority) return;

        Vector3 dir = (enemy.Target.position + Vector3.up * 1.2f - enemy.FirePoint.position).normalized;

        // Extraemos el componente de red del prefab de tu bala enemiga
        NetworkObject prefabNetObj = enemy.ProjectilePrefab.GetComponent<NetworkObject>();

        if (prefabNetObj != null)
        {
            // Usamos el Runner del enemigo para crear la bala oficialmente en todas las pantallas
            NetworkObject projectileObj = enemy.Runner.Spawn(
                prefabNetObj,
                enemy.FirePoint.position,
                Quaternion.LookRotation(dir)
            );

            // Le asignamos el daño a la bala
            EnemyProjectileBehavior proj = projectileObj.GetComponent<EnemyProjectileBehavior>();
            if (proj != null)
            {
                proj.SetDamage(enemy.AttackDamage);
            }

            Debug.Log($"{enemy.name} disparó proyectil al player por la red.");
        }
        else
        {
            Debug.LogError("¡El prefab del proyectil enemigo necesita el componente NetworkObject!");
        }
    }
}