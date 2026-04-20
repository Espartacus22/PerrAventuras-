using UnityEngine;
using UnityEngine.UI;
using Fusion; // ¡Añadido para el multijugador!

public class EnemyStats : NetworkBehaviour
{
    // Datos base del enemigo (vida máxima, XP, drop, etc.)
    public EnemyType enemyData;

    // Barra de vida local que ve cada jugador en pantalla
    [SerializeField] private Slider healthSlider;

    // Vida sincronizada en red:
    // el servidor la modifica y todos los clientes ven el mismo valor
    [Networked] public int currentHealth { get; set; }

    // Bandera local para saber si este NetworkBehaviour ya pasó por Spawned()
    // Esto evita leer propiedades de red antes de tiempo
    private bool _isSpawned;

    // Propiedad pública de compatibilidad para otros scripts
    public int CurrentHealth => GetCurrentHealth();

    public override void Spawned()
    {
        // Marcamos que este enemigo ya fue inicializado correctamente por Fusion
        _isSpawned = true;

        // Solo el servidor (State Authority) decide la vida inicial del enemigo
        if (HasStateAuthority)
        {
            currentHealth = enemyData.maxHealth;
        }

        // Cada cliente ajusta visualmente su barra de vida local
        if (healthSlider != null)
        {
            healthSlider.maxValue = enemyData.maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public override void Render()
    {
        // Si todavía no fue spawneado por Fusion, no intentamos leer la vida en red
        if (!_isSpawned) return;

        // Actualización visual de la barra de vida en pantalla
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    // Indica si el enemigo ya está listo para que otros scripts lean sus datos de red
    public bool IsReady()
    {
        return _isSpawned && Object != null;
    }

    // Método seguro para obtener la vida actual
    // Si todavía no fue inicializado por Fusion, devolvemos 0 para evitar errores
    public int GetCurrentHealth()
    {
        if (!_isSpawned || Object == null)
            return 0;

        return currentHealth;
    }

    public void TakeDamage(int amount)
    {
        // Solo el servidor puede descontar vida
        if (!HasStateAuthority) return;

        currentHealth -= amount;

        // Si la vida llega a 0 o menos, el enemigo muere
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Calcula una posición razonable para spawnear el drop en el suelo
    Vector3 GetOrbSpawnPosition()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out hit, 5f))
        {
            return hit.point + Vector3.up * 0.5f;
        }

        // Fallback por si no encuentra suelo con raycast
        return transform.position + Vector3.up * 1f;
    }

    private void Die()
    {
        // 1) Avisar a sistemas especiales si este enemigo era un núcleo de energía
        EnergyCore core = GetComponent<EnergyCore>();
        if (core != null)
        {
            core.OnCoreDestroyed();
        }

        // 2) Dar recompensas directas al jugador
        PlayerLevel playerLevel = UnityEngine.Object.FindFirstObjectByType<PlayerLevel>();
        RewardOnDeath reward = GetComponent<RewardOnDeath>();

        if (playerLevel != null && reward != null)
        {
            reward.GiveRewards(playerLevel);
        }

        // 3) Spawnear el drop de XP / item en red
        // Solo el servidor puede hacerlo
        if (HasStateAuthority && enemyData.dropPrefab != null)
        {
            Vector3 dropPosition = GetOrbSpawnPosition();
            NetworkObject dropNetObj = enemyData.dropPrefab.GetComponent<NetworkObject>();

            if (dropNetObj != null)
            {
                NetworkObject orbObj = Runner.Spawn(dropNetObj, dropPosition, Quaternion.identity);

                XPOrb orb = orbObj.GetComponent<XPOrb>();
                if (orb != null)
                    orb.xpAmount = enemyData.xpReward;
            }
        }

        // 4) Despawnear oficialmente el enemigo en toda la red
        if (Object != null && Runner != null)
        {
            Runner.Despawn(Object);
        }
    }
}