using UnityEngine;
using Fusion;
using UnityEngine.EventSystems;

public class BasicRangedAttackStrategy : IRangedAttackStrategy
{
    public void Execute(PlayerCombat combat)
    {
        // 1. Bloqueo de UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 2. Validaciones
        if (combat.CharacterData == null || combat.CharacterData.rangedAttacks == null) return;
        if (combat.SelectedRangedIndex < 0 || combat.SelectedRangedIndex >= combat.CharacterData.rangedAttacks.Count) return;

        var attack = combat.CharacterData.rangedAttacks[combat.SelectedRangedIndex];

        // 3. Cooldown
        if (combat.Runner.SimulationTime < combat.LastAttackTime + attack.cooldown) return;
        combat.LastAttackTime = combat.Runner.SimulationTime;

        // 4. Feedback
        if (combat.Animator != null && attack.animation != null)
            combat.Animator.Play(attack.animation.name);

        if (combat.AudioSource != null && attack.sound != null)
            combat.AudioSource.PlayOneShot(attack.sound);

        if (attack.projectilePrefab == null) return;

        // Posición y rotación de origen
        Vector3 spawnPos = combat.FirePoint.position;
        Quaternion spawnRot = combat.FirePoint.rotation;

        // --- 5. BALA VISUAL (CLIENTE) ---
        if (!combat.HasStateAuthority && combat.Runner.IsForward)
        {
            GameObject visualDummy = Object.Instantiate(attack.projectilePrefab.gameObject, spawnPos, spawnRot);

            if (visualDummy.TryGetComponent<NetworkObject>(out var netObj)) Object.Destroy(netObj);
            if (visualDummy.TryGetComponent<ProjectileBehavior>(out var behavior)) Object.Destroy(behavior);

            LocalVisualProjectile lvp = visualDummy.AddComponent<LocalVisualProjectile>();
            ProjectileBehavior originalBehavior = attack.projectilePrefab.GetComponent<ProjectileBehavior>();

            if (originalBehavior != null) lvp.speed = originalBehavior.speed;
            lvp.SetRange(attack.range);
        }

        // --- 6. BALA REAL (SERVIDOR) ---
        if (combat.HasStateAuthority)
        {
            NetworkObject projectileNetObj = combat.Runner.Spawn(
                attack.projectilePrefab.GetComponent<NetworkObject>(),
                spawnPos,
                spawnRot,
                combat.Object.InputAuthority
            );

            ProjectileBehavior pb = projectileNetObj.GetComponent<ProjectileBehavior>();
            if (pb != null)
            {
                pb.SetRange(attack.range);
                pb.SetDamage(attack.damage);
            }
        }
    }
}