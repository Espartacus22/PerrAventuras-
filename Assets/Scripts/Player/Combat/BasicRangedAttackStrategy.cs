using UnityEngine;

public class BasicRangedAttackStrategy : IRangedAttackStrategy
{
    public void Execute(PlayerCombat combat)
    {
        if (combat.CharacterData == null || combat.CharacterData.rangedAttacks == null) return;
        if (combat.SelectedRangedIndex < 0 || combat.SelectedRangedIndex >= combat.CharacterData.rangedAttacks.Count) return;

        var attack = combat.CharacterData.rangedAttacks[combat.SelectedRangedIndex];

        if (Time.time < combat.LastAttackTime + attack.cooldown) return;
        combat.LastAttackTime = Time.time;

        if (combat.Animator != null && attack.animation != null)
            combat.Animator.Play(attack.animation.name);

        if (combat.AudioSource != null && attack.sound != null)
            combat.AudioSource.PlayOneShot(attack.sound);

        Vector3 shootDirection = combat.transform.forward;

        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
                shootDirection = (hit.point - combat.transform.position).normalized;
        }

        if (attack.projectilePrefab == null) return;

        GameObject projectile = Object.Instantiate(
            attack.projectilePrefab,
            combat.transform.position + shootDirection,
            Quaternion.LookRotation(shootDirection)
        );

        ProjectileBehavior pb = projectile.GetComponent<ProjectileBehavior>();
        if (pb != null)
        {
            pb.SetRange(attack.range);
            pb.SetDamage(attack.damage);
        }
    }
}
