using Fusion;
using Networking;
using UnityEngine;

public class Projectile : NetworkBehaviour
{

    [Networked] private TickTimer life { set; get; }
    [SerializeField] private float _speed;
    public void InitProjectile ()
    {
        life = TickTimer.CreateFromSeconds(Runner, delayInSeconds: 3);
    }
    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            { 

            transform.position += transform.forward * Runner.DeltaTime* _speed;
        }
        else 
        { Runner.Despawn(Object);
        }
    }
}
