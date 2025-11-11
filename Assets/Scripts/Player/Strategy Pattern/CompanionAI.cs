using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FollowPlayer))]
[RequireComponent(typeof(ProjectileLocal))]
public class CompanionAI : MonoBehaviour
{
    public Transform target; // objetivo (enemigo o null)
    public bool stayCommand = false;
    public bool isAggressive = false;
    public bool isDefensive = false;

    private IAttackStrategy attackStrategy;
    private ProjectileLocal shooter;
    private FollowPlayer follow;

    void Start()
    {
        shooter = GetComponent<ProjectileLocal>();
        follow = GetComponent<FollowPlayer>();
        follow.enabled = true;
    }

    void Update()
    {
        // F para quedarse quieto
        if (Input.GetKeyDown(KeyCode.F))
        {
            stayCommand = !stayCommand;
            follow.enabled = !stayCommand; // Evita que NavMesh y AI se peleen
        }

        // Si está quieto y agresivo → ejecuta ataque
        if (stayCommand && isAggressive && attackStrategy != null && target != null)
        {
            attackStrategy.Execute(shooter, target);
        }
        // Cambiar modos de IA (1=corto, 2=largo, 3=defensa)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isAggressive = true;
            isDefensive = false;
            attackStrategy = new ShortRangeAttackStrategy();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            isAggressive = true;
            isDefensive = false;
            attackStrategy = new LongRangeAttackStrategy();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            isAggressive = false;
            isDefensive = true;
            attackStrategy = new DefenseStrategy();
        }        
    }
}
