using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FollowPlayer))]
public class CompanionAI : MonoBehaviour
{
    public Transform target; // enemigo o null

    public bool stayCommand = false;
    public bool isAggressive = false;
    public bool isDefensive = false;

    private IAttackStrategy attackStrategy;
    private ICompanionCombat combat;
    private FollowPlayer follow;

    void Start()
    {
        follow = GetComponent<FollowPlayer>();
        follow.enabled = true;

        combat = GetComponent<ICompanionCombat>();
        if (combat == null)
        {
            Debug.LogWarning("CompanionAI: No hay componente que implemente ICompanionCombat en este compañero.");
        }
    }

    void Update()
    {
        // Si está quieto, agresivo, tiene estrategia y target → ataca
        if (stayCommand && isAggressive && attackStrategy != null && target != null && combat != null)
        {
            attackStrategy.Execute(combat, target);
        }

        // Si está en modo defensa → bloquear (escudo ON)
        if (isDefensive && combat != null)
        {
            combat.Block(true);
        }
        else if (combat != null)
        {
            combat.Block(false);
        }
    }

    // --------- API de órdenes (para que PJ1 se lo diga) ---------

    public void ToggleStay()
    {
        stayCommand = !stayCommand;
        follow.enabled = !stayCommand;
    }

    public void SetShortRangeAggressive()
    {
        isAggressive = true;
        isDefensive = false;
        attackStrategy = new Atk_ShortStrategy();
    }

    public void SetLongRangeAggressive()
    {
        isAggressive = true;
        isDefensive = false;
        attackStrategy = new Atk_LongStrategy();
    }

    public void SetDefensive()
    {
        isAggressive = false;
        isDefensive = true;
        attackStrategy = new DefenseStrategy();
    }
}
