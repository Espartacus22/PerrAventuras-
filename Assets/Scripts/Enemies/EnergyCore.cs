using UnityEngine;

public class EnergyCore : MonoBehaviour
{
    public int maxHP = 50;
    int currentHP;

    [Tooltip("Torretas que dependen de este núcleo.")]
    public LaserTurret[] linkedTurrets;

    [Header("Opcional VFX")]
    public GameObject activeVfx;
    public GameObject destroyedVfx;

    void Start()
    {
        currentHP = maxHP;
        Debug.Log($"EnergyCore spawn con {currentHP} HP");
        SetActiveVisual(true);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log("EnergyCore recibió " + amount + " de daño. HP restante: " + currentHP);
        
        if (currentHP <= 0)
            DestroyCore();
    }

    void DestroyCore()
    {
        Debug.Log("EnergyCore destruido, apagando torretas…");

        // Apagar todas las torretas vinculadas
        foreach (var t in linkedTurrets)
        {
            if (t == null) continue;
            t.DisableTurret();
        }

        // Opcional: destruir el núcleo
        // Destroy(gameObject);
    }

    void SetActiveVisual(bool active)
    {
        if (activeVfx != null) activeVfx.SetActive(active);
    }
}
