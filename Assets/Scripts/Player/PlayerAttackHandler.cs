using UnityEngine;

[RequireComponent(typeof(ProjectileLocal))]
public class PlayerAttackHandler : MonoBehaviour
{
    private IAttackStrategy attackStrategy;
    private ProjectileLocal shooter;

    void Start()
    {
        shooter = GetComponent<ProjectileLocal>();
        attackStrategy = new ShortRangeAttackStrategy(); // por defecto
    }

    void Update()
    {
        // Click izquierdo = ataque corto
        if (Input.GetMouseButtonDown(0))
        {
            attackStrategy = new ShortRangeAttackStrategy();
            attackStrategy.Execute(shooter);
        }

        // Click derecho = ataque largo
        if (Input.GetMouseButtonDown(1))
        {
            attackStrategy = new LongRangeAttackStrategy();
            attackStrategy.Execute(shooter);
        }
    }
}
