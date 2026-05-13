using UnityEngine;
using Fusion;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerCombat : NetworkBehaviour
{
    [Header("Datos del personaje")]
    [SerializeField] private CharacterType characterData;

    [Header("Ataques seleccionados")]
    [SerializeField] private int selectedMeleeIndex = 0;
    [SerializeField] private int selectedRangedIndex = 0;

    [Header("Punto de ataque")]
    [SerializeField] private Transform firePoint;

    public Transform FirePoint => firePoint;

    private Animator animator;
    private AudioSource audioSource;
    public float LastAttackTime { get; set; } 
    private PlayerInputHandler inputHandler;
    private IMeleeAttackStrategy meleeStrategy;
    private IRangedAttackStrategy rangedStrategy;

    public CharacterType CharacterData => characterData;
    public int SelectedMeleeIndex => selectedMeleeIndex;
    public int SelectedRangedIndex => selectedRangedIndex;
    public Animator Animator => animator;
    public AudioSource AudioSource => audioSource;

    // --- TEMPORIZADORES DE RED PARA PREDICCIÓN EXACTA ---
    [Networked] public TickTimer RangedCooldown { get; set; }
    [Networked] public TickTimer MeleeCooldown { get; set; }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        inputHandler = GetComponent<PlayerInputHandler>();

        if (characterData == null)
        {
            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null)
                characterData = movement.characterData;
        }

        meleeStrategy = new BasicMeleeAttackStrategy();
        rangedStrategy = new BasicRangedAttackStrategy();
    }

    public override void FixedUpdateNetwork()
    {
        if (inputHandler.MeleePressed)
        {
            meleeStrategy.Execute(this);
        }

        if (inputHandler.RangedPressed)
        {
            rangedStrategy.Execute(this);
        }
    }

    public void SetMeleeStrategy(IMeleeAttackStrategy newStrategy)
    {
        meleeStrategy = newStrategy;
    }

    public void SetRangedStrategy(IRangedAttackStrategy newStrategy)
    {
        rangedStrategy = newStrategy;
    }

    // --- EL RPC PARA EL DISPARO SEGURO DESDE EL CLIENTE ---
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestFire(Vector3 spawnPosition, Vector3 shootDirection)
    {
        if (CharacterData == null || CharacterData.rangedAttacks == null) return;

        var attack = CharacterData.rangedAttacks[SelectedRangedIndex];

        // Se usa directamente el GameObject del prefab para mayor seguridad en Fusion
        if (attack.projectilePrefab != null)
        {
            NetworkObject projectileNetObj = Runner.Spawn(
                attack.projectilePrefab,
                spawnPosition,
                Quaternion.LookRotation(shootDirection),
                Object.InputAuthority
            );

            if (projectileNetObj != null)
            {
                ProjectileBehavior pb = projectileNetObj.GetComponent<ProjectileBehavior>();
                if (pb != null)
                {
                    pb.SetRange(attack.range);
                    pb.SetDamage(attack.damage);
                }
            }
        }
    }
}