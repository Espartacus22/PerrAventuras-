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

        if (attack.projectilePrefab == null) return;

        Vector3 spawnPosition = combat.transform.position + combat.transform.forward * 1.0f + Vector3.up * 0.8f;
        Vector3 shootDirection = combat.transform.forward;

        if (Camera.main != null)
        {
            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
                targetPoint = hit.point;
            else
                targetPoint = ray.GetPoint(200f);

            shootDirection = (targetPoint - spawnPosition).normalized;
        }

        // Opcional: hacer que el personaje mire hacia donde dispara, sin inclinarse para arriba/abajo
        Vector3 flatDir = new Vector3(shootDirection.x, 0f, shootDirection.z).normalized;
        if (flatDir.sqrMagnitude > 0.001f)
            combat.transform.forward = flatDir;

        GameObject projectile = Object.Instantiate(
            attack.projectilePrefab,
            spawnPosition,
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
