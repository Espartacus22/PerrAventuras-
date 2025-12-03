using UnityEngine;

[System.Serializable]
public class MeleeAttackData
{
    public string attackName;
    public float damage;
    public float cooldown;
    public float range = 2f;
    public AnimationClip animation;
    public AudioClip sound;
    public GameObject impactEffectPrefab;
    public bool canChainCombo;
    public int nextComboIndex;
    public int requiredLevel = 1;
}
