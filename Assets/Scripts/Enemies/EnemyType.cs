using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Enemy/EnemyType")]
public class EnemyType : ScriptableObject
{
    public string enemyName;
    public int maxHealth;
    public float moveSpeed;
    public float chaseRange;
    public int xpReward;
    public GameObject dropPrefab;
}
