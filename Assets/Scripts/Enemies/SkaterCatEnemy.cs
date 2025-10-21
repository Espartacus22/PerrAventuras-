using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SkaterCatEnemy : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3f;
    private int idx = 0;

    [Header("Detección")]
    public Transform player;
    public float outerDetectionRange = 16f;     // rango para empezar a perseguir/disparar
    public float innerDetectionRange = 6f;      // rango que activa countdown (ventana)
    public float initialDelayInInner = 2.5f;    // segundos de ventaja antes de disparar al entrar al inner range
    public float preferredDistance = 10f;

    [Header("Disparo")]
    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public float shootInterval = 2f;

    [Header("HP / Furia")]
    public int maxHP = 1;
    private int hp;
    private bool inFury = false;
    public float furyTimeout = 3f;

    // internals
    private CharacterController controller;
    private float lastShotTime = 0f;
    private bool isCountingDown = false;
    private bool playerInOuter = false;
    private Vector3 gravityVel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        hp = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
