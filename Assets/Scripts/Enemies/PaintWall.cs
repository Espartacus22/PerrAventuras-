using UnityEngine;

public class PaintWall : MonoBehaviour
{
    public int maxHP = 30;
    public float lifeTime = 15f; // si querés que desaparezca solo

    private int currentHP;

    void Start()
    {
        currentHP = maxHP;
        if (lifeTime > 0)
            Destroy(gameObject, lifeTime);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
