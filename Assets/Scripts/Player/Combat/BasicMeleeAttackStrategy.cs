using UnityEngine;
using Fusion;

public class BasicMeleeAttackStrategy : IMeleeAttackStrategy
{
    public void Execute(PlayerCombat combat)
    {
        if (combat.CharacterData == null || combat.CharacterData.meleeAttacks == null) return;
        if (combat.SelectedMeleeIndex < 0 || combat.SelectedMeleeIndex >= combat.CharacterData.meleeAttacks.Count) return;

        var attack = combat.CharacterData.meleeAttacks[combat.SelectedMeleeIndex];

        // --- VALIDACIÓN DE COOLDOWN DE FUSION ---
        if (!combat.MeleeCooldown.ExpiredOrNotRunning(combat.Runner)) return;
        combat.MeleeCooldown = TickTimer.CreateFromSeconds(combat.Runner, attack.cooldown);

        if (combat.Animator != null)
        {
            // NO uses combat.Animator.SetTrigger directamente.
            // Llama a una función que use el NetworkMecanimAnimator
            var netMecanim = combat.GetComponent<NetworkMecanimAnimator>();
            if (netMecanim != null && netMecanim.Animator != null)
            {
                netMecanim.Animator.SetTrigger("AttackTrigger");
            }
        }

        if (combat.AudioSource != null && attack.sound != null)
            combat.AudioSource.PlayOneShot(attack.sound);

        if (attack.impactEffectPrefab != null)
            Object.Instantiate(attack.impactEffectPrefab, combat.transform.position + combat.transform.forward, combat.transform.rotation);

        // --- EL DAÑO SIGUE EXCLUSIVO DEL SERVIDOR ---
        if (combat.HasStateAuthority)
        {
            Collider[] hitEnemies = Physics.OverlapSphere(
                combat.transform.position + combat.transform.forward * attack.range * 0.5f,
                attack.range * 0.5f
            );

            foreach (Collider col in hitEnemies)
            {
                if (col.CompareTag("Enemy"))
                {
                    EnemyStats enemy = col.GetComponent<EnemyStats>();
                    if (enemy != null)
                    {
                        int damageDealt = Mathf.RoundToInt(attack.damage);
                        enemy.TakeDamage(damageDealt);
                    }
                }
            }
        }
    }
}