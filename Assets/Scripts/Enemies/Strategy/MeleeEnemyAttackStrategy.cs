using UnityEngine;

public class MeleeEnemyAttackStrategy : IEnemyAttackStrategy
{
    public void Execute(EnemyController enemy)
    {
        if (enemy.Target == null) return;

        float dist = Vector3.Distance(enemy.transform.position, enemy.Target.position);
        if (dist > enemy.AttackRange) return;

        PlayerLevel playerLevel = enemy.Target.GetComponent<PlayerLevel>();
        if (playerLevel != null)
        {
            playerLevel.TakeDamage(enemy.AttackDamage);
            Debug.Log($"{enemy.name} golpeó al jugador por {enemy.AttackDamage}");
        }
    }
}
