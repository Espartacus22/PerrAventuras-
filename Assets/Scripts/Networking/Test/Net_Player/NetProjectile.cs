using Fusion;
using UnityEngine;

public class NetProjectile : NetworkBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 3f;

    [Networked] private Vector3 Direction { get; set; }
    [Networked] private PlayerRef OwnerRef { get; set; }
    [Networked] private TickTimer LifeTimer { get; set; }

    public void Init(Vector3 dir, PlayerRef owner)
    {
        if (!HasStateAuthority)
            return;

        Direction = dir.normalized;
        OwnerRef = owner;
        LifeTimer = TickTimer.CreateFromSeconds(Runner, lifetime);
    }

    public override void FixedUpdateNetwork()
    {
        transform.position += Direction * speed * Runner.DeltaTime;

        if (HasStateAuthority && LifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority)
            return;

        NetPlayerHealth health = other.GetComponentInParent<NetPlayerHealth>();

        if (health == null)
            return;

        if (health.Object != null && health.Object.InputAuthority == OwnerRef)
            return;

        health.TakeDamage(damage);
        Debug.Log($"Projectile hit player: {health.Object.name}");
        Runner.Despawn(Object);
    }
}
