using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Atributos
    public float moveSpeed = 5f;
    public Transform target;
    public GameObject weapon;

    private EnemyState currentState = EnemyState.Idle;
    private HealthSystem healthSystem;
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead,
    }

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null)
        {
            Debug.LogError("HealthSystem error");
        }
    }

    void Update()
    {
        if (healthSystem != null && healthSystem.GetHealth() <= 0 && currentState != EnemyState.Dead)
        {
            SetState(EnemyState.Dead);
            OnDeath();
            Debug.Log("Enemy dehath");
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Patrol:
                break;
            case EnemyState.Chase:
                if (target != null)
                {
                    MoveTo(target.position);
                }
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Dead:
                break;
        }
    }
    public void MoveTo(Vector2 position)
    {
        transform.position = Vector2.MoveTowards(transform.position, position, moveSpeed * Time.deltaTime);
    }

    public void Pursue(Transform newTarget)
    {
        target = newTarget;
        SetState(EnemyState.Chase);
    }
    public void Attack()
    {
        if (weapon != null)
        {
            Debug.Log("Enemy attack");
        }
    }
    public void SetState(EnemyState newState)
    {
        currentState = newState;
    }
    public void OnDeath()
    {
        Debug.Log("Enemy death");
    }
}
