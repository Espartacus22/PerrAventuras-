using System.Collections;
using UnityEngine;

public class ShortRangeAttackStrategy : IAttackStrategy
{
    public float range = 2f;
    public float dashDuration = 0.2f;

    public void Execute(ProjectileLocal shooter, Transform target = null)
    {
        if (shooter == null) return;

        PlayerLocal ctx = shooter.GetComponent<PlayerLocal>();
        if (ctx == null) return;

        // Si no hay target buscá el más cercano con tag "Enemy"
        if (target == null)
        {
            target = FindClosestEnemy(ctx.transform);
            if (target == null) return;
        }

        float dist = Vector3.Distance(ctx.transform.position, target.position);
        if (dist <= range)
        {
            Vector3 dir = (target.position - ctx.transform.position).normalized;
            ctx.StartCoroutine(PerformCharge(ctx, dir));
        }
    }

    private IEnumerator PerformCharge(PlayerLocal ctx, Vector3 dir)
    {
        float timer = 0f;
        while (timer < dashDuration)
        {
            // mover usando CharacterController (PlayerLocal debe exponer controller)
            ctx.controller.Move(dir * ctx.characterData.dashSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Aquí podrías aplicar daño a quien colisione (si detectás colisión)
    }

    private Transform FindClosestEnemy(Transform self)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closest = null;
        float minDist = float.MaxValue;
        foreach (var e in enemies)
        {
            float d = Vector3.Distance(self.position, e.transform.position);
            if (d < minDist) { minDist = d; closest = e.transform; }
        }
        return closest;
    }
}
