using UnityEngine;

[CreateAssetMenu(fileName = "CharacterType", menuName = "Scriptable Objects/CharacterType")]
public class CharacterType : ScriptableObject
{

    [Header("General Settings / Configuraci�n general")]
    public string characterName = "Unnamed";
    public GameObject characterModel;

    [Header("Movimiento")]
    public float walkSpeed = 5f;           // Velocidad base de caminata
    public float runMultiplier = 1.5f;     // Multiplicador al correr
    public float jumpForce = 7f;           // Fuerza de salto

    [Tooltip("Fuerza y duraci�n del dash (si aplica)")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.25f;

    [Header("Combat / Combate")]
    [Tooltip("Da�o de ataque cuerpo a cuerpo")]
    public int meleeDamage = 1;

    [Tooltip("Da�o de ataque a distancia (proyectil)")]
    public int rangedDamage = 1;

    [Tooltip("Velocidad del proyectil (si aplica)")]
    public float projectileSpeed = 12f;

    [Tooltip("Tiempo entre ataques (en segundos)")]
    public float attackCooldown = 0.5f;

    [Header("Health & Shield / Vida y escudo")]
    [Tooltip("Cantidad m�xima de vida del personaje")]
    public int maxHP = 5;

    [Tooltip("Cantidad m�xima de escudo del personaje")]
    public int maxShield = 3;

    [Tooltip("Velocidad de regeneraci�n de escudo (puntos por segundo)")]
    public float shieldRegenRate = 0.5f;

    [Tooltip("Retraso antes de comenzar la regeneraci�n del escudo")]
    public float shieldRegenDelay = 3f;

    [Header("Abilities / Habilidades especiales")]
    public bool canDoubleJump = false;
    public bool canBlock = false;
    public bool canDash = false;

    [Tooltip("Si puede realizar ataques m�gicos o habilidades especiales")]
    public bool canUseMagic = false;

    [Header("Audio & Visuals / Sonidos y efectos")]
    public AudioClip attackSound;
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip hitSound;

    [Header("Animaciones / Animator Parameters")]
    public string idleAnim = "Idle";
    public string walkAnim = "Walk";
    public string attackAnim = "Attack";
    public string deathAnim = "Death";
}