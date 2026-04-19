using UnityEngine;
using Fusion; // ¡Añadido!

public class BasicRangedAttackStrategy : IRangedAttackStrategy
{
    public void Execute(PlayerCombat combat)
    {
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
            Vector3 spawnPosition = combat.transform.position + combat.transform.forward * 1.0f + Vector3.up * 0.8f;
            Vector3 shootDirection = combat.transform.forward;

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
