using UnityEngine;

[System.Serializable]
public class RangedAttackData
{
    public string attackName;
    public float damage = 10f;
    public float cooldown = 1f;
    public float range = 10f;
    public AnimationClip animation;
    public AudioClip sound;
    public GameObject projectilePrefab;
    public int requiredLevel = 1;
}