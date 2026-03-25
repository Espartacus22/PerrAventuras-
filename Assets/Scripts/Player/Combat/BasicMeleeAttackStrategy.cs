using UnityEngine;

public class BasicMeleeAttackStrategy : IMeleeAttackStrategy
{
    public void Execute(PlayerCombat combat)
    {
        if (combat.CharacterData == null || combat.CharacterData.meleeAttacks == null) return;
        if (combat.SelectedMeleeIndex < 0 || combat.SelectedMeleeIndex >= combat.CharacterData.meleeAttacks.Count) return;

        var attack = combat.CharacterData.meleeAttacks[combat.SelectedMeleeIndex];

        if (Time.time < combat.LastAttackTime + attack.cooldown) return;
        combat.LastAttackTime = Time.time;

        if (combat.Animator != null && attack.animation != null)
            combat.Animator.Play(attack.animation.name);

        if (combat.AudioSource != null && attack.sound != null)
            combat.AudioSource.PlayOneShot(attack.sound);

        if (attack.impactEffectPrefab != null)
            Object.Instantiate(attack.impactEffectPrefab, combat.transform.position + combat.transform.forward, combat.transform.rotation);

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
