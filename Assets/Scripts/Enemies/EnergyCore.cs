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
        SetActiveVisual(true);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
            DestroyCore();
    }

    void DestroyCore()
    {
        foreach (var t in linkedTurrets)
        {
            if (t != null)
                t.DisableTurret();
        }

        SetActiveVisual(false);

        if (destroyedVfx != null)
        {
            Instantiate(destroyedVfx, transform.position, Quaternion.identity);
        }

        // podés dejar el núcleo roto en la escena, o destruirlo:
        // Destroy(gameObject);
    }

    void SetActiveVisual(bool active)
    {
        if (activeVfx != null) activeVfx.SetActive(active);
    }
}
