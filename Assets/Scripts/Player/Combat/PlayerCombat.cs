using Fusion;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private CharacterType characterData;
    [SerializeField] private int selectedMeleeIndex = 0;
    [SerializeField] private int selectedRangedIndex = 0;
    [SerializeField] private Transform firePoint;

    public Transform FirePoint => firePoint;
    public Animator Animator { get; private set; }
    public AudioSource AudioSource { get; private set; }
    public float LastAttackTime { get; set; }

    private PlayerInputHandler inputHandler;
    private IMeleeAttackStrategy meleeStrategy;
    private IRangedAttackStrategy rangedStrategy;

    public CharacterType CharacterData => characterData;
    public int SelectedMeleeIndex => selectedMeleeIndex;
    public int SelectedRangedIndex => selectedRangedIndex;

    [Networked] public TickTimer RangedCooldown { get; set; }
    [Networked] public TickTimer MeleeCooldown { get; set; }

    private void Awake()
    {
        // Esto buscará el Animator en el hijo (el FBX) y lo asignará.
        // Como tu BasicRangedAttackStrategy usa "combat.Animator", 
        // al tener esto asignado aquí, tu estrategia ya no fallará.
        Animator = GetComponentInChildren<Animator>();
        AudioSource = GetComponent<AudioSource>();
        inputHandler = GetComponent<PlayerInputHandler>();


        meleeStrategy = new BasicMeleeAttackStrategy();
        rangedStrategy = new BasicRangedAttackStrategy();
    }
    public void TriggerAttackAnimation(string triggerName)
    {
        var netMecanim = GetComponent<NetworkMecanimAnimator>();
        if (netMecanim != null && netMecanim.Animator != null)
        {
            netMecanim.Animator.SetTrigger(triggerName);
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (Animator == null || !Animator.gameObject.activeInHierarchy) return;

        if (inputHandler.MeleePressed) meleeStrategy.Execute(this);
        if (inputHandler.RangedPressed) rangedStrategy.Execute(this);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestFire(Vector3 spawnPosition, Vector3 shootDirection)
    {
        if (CharacterData == null || CharacterData.rangedAttacks == null) return;
        var attack = CharacterData.rangedAttacks[SelectedRangedIndex];

        if (attack.projectilePrefab != null)
        {
            Runner.Spawn(attack.projectilePrefab, spawnPosition, Quaternion.LookRotation(shootDirection), Object.InputAuthority);
        }
    }
}

