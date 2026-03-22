using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyType")]
public class EnemyType : ScriptableObject
{
    public string enemyName;
    public int maxHealth;
    public float moveSpeed = 3.5f;
    public float chaseRange = 10f;
    public int xpReward = 25;
    public GameObject dropPrefab;

    // ATAQUE MELEE 100% EDITABLE DESDE EL INSPECTOR
    [Header("Melee Attack")]
    public float meleeDamage = 20f;
    public float meleeCooldown = 1.5f;
    public float meleeRange = 1.8f;

    // Opcional: si querés animación de ataque del enemigo
    public AnimationClip attackAnimation;
    public AudioClip attackSound;
}
