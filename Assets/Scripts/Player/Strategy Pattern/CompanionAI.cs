using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FollowPlayer))]
[RequireComponent(typeof(ProjectileLocal))]
public class CompanionAI : MonoBehaviour
{
    public Transform target;              // objetivo (player o enemigo)
    public bool stayCommand = false;      // si está quieto
    public bool isAggressive = false;
    public bool isDefensive = false;

    private IAttackStrategy attackStrategy;
    private ProjectileLocal shooter;
    private FollowPlayer follow;

    void Start()
    {
        shooter = GetComponent<ProjectileLocal>();
        follow = GetComponent<FollowPlayer>();
        UpdateStrategy();
    }

    void Update()
    {
        // Toggle quedarse quieto (F)
        if (Input.GetKeyDown(KeyCode.F))
        {
            stayCommand = !stayCommand;
            if (follow != null) follow.enabled = !stayCommand;
        }

        // Cambiar modos (ejemplo 1=short, 2=long, 3=defense)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isAggressive = true;
            isDefensive = false;
            attackStrategy = new ShortRangeAttackStrategy();
            Debug.Log("Modo: Corto alcance");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            isAggressive = true;
            isDefensive = false;
            attackStrategy = new LongRangeAttackStrategy();
            Debug.Log("Modo: Largo alcance");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            isAggressive = false;
            isDefensive = true;
            attackStrategy = new DefenseStrategy();
            Debug.Log("Modo: Defensa");
        }

        // Si está quieto y agresivo, ejecuta la estrategia contra target
        if (stayCommand && isAggressive && attackStrategy != null)
        {
            Debug.Log("Ejecuto estrategia: " + attackStrategy.GetType().Name);
            attackStrategy.Execute(shooter, target);
        }
    }

    void UpdateStrategy()
    {
        if (isAggressive) attackStrategy = new ShortRangeAttackStrategy();
        else if (isDefensive) attackStrategy = new DefenseStrategy();
        else attackStrategy = null;
    }
}
