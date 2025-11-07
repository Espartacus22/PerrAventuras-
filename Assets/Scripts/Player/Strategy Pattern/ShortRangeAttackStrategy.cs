using System.Collections;
using UnityEngine;

public class ShortRangeAttackStrategy : IAttackStrategy
{
    public float damage = 10f;
    public float range = 2f;
    public float dashForce = 12f;

    public void Execute(ProjectileLocal shooter, Transform target = null)
    {
        if (shooter == null || shooter.GetComponent<PlayerLocal>() == null) return;

        var ctx = shooter.GetComponent<PlayerLocal>();
        if (target == null) target = FindClosestTarget(ctx.transform);

        if (target == null) return;

        float dist = Vector3.Distance(ctx.transform.position, target.position);

        if (dist <= range)
        {
            Vector3 dir = (target.position - ctx.transform.position).normalized;
            ctx.StartCoroutine(ChargeDash(ctx, dir));
        }
    }

    private IEnumerator ChargeDash(PlayerLocal ctx, Vector3 dir)
    {
        float dashTime = 0.2f;
        float timer = 0f;
        while (timer < dashTime)
        {
            ctx.controller.Move(dir * ctx.characterData.dashSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Aquí aplicar daño si el objetivo tiene collider con salud
        // Ejemplo: other.GetComponent<PlayerHealthLocal>()?.TakeDamage(damage);
    }

    private Transform FindClosestTarget(Transform self)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (var e in enemies)
        {
            float dist = Vector3.Distance(self.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = e.transform;
            }
        }
        return closest;
    }
}
