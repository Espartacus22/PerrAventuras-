using UnityEngine;
using Fusion;
using UnityEngine.EventSystems;

public class BasicRangedAttackStrategy : IRangedAttackStrategy
{
    public void Execute(PlayerCombat combat)
    {
        // 1. Bloqueo si el mouse está sobre la UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 2. Validaciones de datos
        if (combat.CharacterData == null || combat.CharacterData.rangedAttacks == null) return;
        if (combat.SelectedRangedIndex < 0 || combat.SelectedRangedIndex >= combat.CharacterData.rangedAttacks.Count) return;

        var attack = combat.CharacterData.rangedAttacks[combat.SelectedRangedIndex];

        // 3. Gestión de Cooldown (Usando LastAttackTime de PlayerCombat)
        if (combat.Runner.SimulationTime < combat.LastAttackTime + attack.cooldown) return;
        combat.LastAttackTime = combat.Runner.SimulationTime;

        // 4. Feedback visual y sonoro
        if (combat.Animator != null && attack.animation != null)
            combat.Animator.Play(attack.animation.name);

        if (combat.AudioSource != null && attack.sound != null)
            combat.AudioSource.PlayOneShot(attack.sound);

        if (attack.projectilePrefab == null) return;

        // --- 5. BALA VISUAL (CLIENTE) ---
        if (!combat.HasStateAuthority && combat.Runner.IsForward)
        {
            GameObject visualDummy = Object.Instantiate(
                attack.projectilePrefab.gameObject,
                combat.FirePoint.position,
                combat.FirePoint.rotation
            );

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
                combat.FirePoint.position,
                combat.FirePoint.rotation,
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