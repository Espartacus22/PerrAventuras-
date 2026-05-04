using UnityEngine;
using Fusion;
using UnityEngine.EventSystems; // <--- 1. IMPORTANTE: Necesario para detectar la UI

public class BasicRangedAttackStrategy : IRangedAttackStrategy
{
    public void Execute(PlayerCombat combat)
    {
        // --- NUEVO BLOQUEO DE UI ---
        // Si el mouse está sobre un casillero del inventario u otro elemento de UI, abortamos el ataque.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        // ---------------------------

        if (combat.CharacterData == null || combat.CharacterData.rangedAttacks == null) return;
        if (combat.SelectedRangedIndex < 0 || combat.SelectedRangedIndex >= combat.CharacterData.rangedAttacks.Count) return;

        var attack = combat.CharacterData.rangedAttacks[combat.SelectedRangedIndex];

        // Cooldown multijugador
        if (combat.Runner.SimulationTime < combat.LastAttackTime + attack.cooldown) return;
        combat.LastAttackTime = combat.Runner.SimulationTime;

        if (combat.Animator != null && attack.animation != null)
            combat.Animator.Play(attack.animation.name);

        if (combat.AudioSource != null && attack.sound != null)
            combat.AudioSource.PlayOneShot(attack.sound);

        if (attack.projectilePrefab == null) return;

        // CANDADO DE RED: Solo el servidor spawnea la bala para que no se dupliquen
        if (combat.HasStateAuthority)
        {
            Transform firePoint = combat.FirePoint;

            Vector3 spawnPosition = firePoint.position;
            Vector3 shootDirection = firePoint.forward;

            NetworkObject prefabNetObj = attack.projectilePrefab.GetComponent<NetworkObject>();
            if (prefabNetObj != null)
            {
                NetworkObject projectileNetObj = combat.Runner.Spawn(
                    prefabNetObj,
                    spawnPosition,
                    Quaternion.LookRotation(shootDirection),
                    combat.Object.InputAuthority
                );

                ProjectileBehavior pb = projectileNetObj.GetComponent<ProjectileBehavior>();
                if (pb != null)
                {
                    pb.SetRange(attack.range);
                    pb.SetDamage(attack.damage);
                }
            }
            else
            {
                Debug.LogError("¡El prefab de la bala necesita un NetworkObject!");
            }
        }
    }
}
