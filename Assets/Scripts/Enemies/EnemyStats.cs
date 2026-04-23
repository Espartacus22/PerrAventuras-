using UnityEngine;
using UnityEngine.UI;
using Fusion; // ¡Añadido para el multijugador!

public class EnemyStats : NetworkBehaviour
{
    public EnemyType enemyData;
    [SerializeField] private Slider healthSlider;

    // 1. ¡VIDA EN RED! Ahora todos los jugadores ven exactamente la misma vida del enemigo
    [Networked] public int currentHealth { get; set; }

    // 2. ¡EL ARREGLO DEL ERROR! Restauramos estas propiedades para que los scripts del Jefe no se rompa.

    private bool _isSpawned;

    public override void Spawned()
    {
        _isSpawned = true;

        // Al iniciar, solo el servidor dicta cuánta vida máxima tiene
        if (HasStateAuthority)
        {
            currentHealth = enemyData.maxHealth;
        }

        // Todos ajustan el tamaño de su barra de vida local
        if (healthSlider != null)
        {
            healthSlider.maxValue = enemyData.maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public int GetCurrentHealth()
    {
        if (!_isSpawned || Object == null)
            return 0;

        return currentHealth;
    }

    public int CurrentHealth => GetCurrentHealth();

    // Render() se ejecuta constantemente en la pantalla de todos los jugadores. Ideal para actualizar UIs.
    public override void Render()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        // CANDADO DE RED: Solo el servidor resta la vida
        if (!HasStateAuthority) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    Vector3 GetOrbSpawnPosition()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out hit, 5f))
        {
            return hit.point + Vector3.up * 0.5f;
        }
        return transform.position + Vector3.up * 1f;
    }

    private void Die()
    {
        // 1. Avisos a otros sistemas (Núcleo)
        EnergyCore core = GetComponent<EnergyCore>();
        if (core != null)
        {
            core.OnCoreDestroyed();
        }

        // 2. Dar rewards directos al player (HP / Shield / Coins)
        // Usamos UnityEngine.Object para que Fusion no se confunda
        PlayerLevel playerLevel = UnityEngine.Object.FindFirstObjectByType<PlayerLevel>();
        RewardOnDeath reward = GetComponent<RewardOnDeath>();
        if (playerLevel != null && reward != null)
        {
            reward.GiveRewards(playerLevel);
        }

        // 3. Drop de XP Orb (Versión Multijugador)
        if (HasStateAuthority && enemyData.dropPrefab != null)
        {
            Vector3 dropPosition = GetOrbSpawnPosition();
            NetworkObject dropNetObj = enemyData.dropPrefab.GetComponent<NetworkObject>();

            if (dropNetObj != null)
            {
                // Usamos el Runner de este enemigo para spawnear el orbe
                NetworkObject orbObj = Runner.Spawn(dropNetObj, dropPosition, Quaternion.identity);

                XPOrb orb = orbObj.GetComponent<XPOrb>();
                if (orb != null) orb.xpAmount = enemyData.xpReward;
            }
        }

        // 4. Destrucción oficial del enemigo en toda la red
        if (Object != null && Runner != null)
        {
            Runner.Despawn(Object);
        }
    }
}