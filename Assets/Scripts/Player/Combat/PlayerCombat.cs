using UnityEngine;

using Fusion; // ¡Añadido para red!



[RequireComponent(typeof(PlayerInputHandler))]

public class PlayerCombat : NetworkBehaviour // Cambiado a NetworkBehaviour

{

    [Header("Datos del personaje")]

    [SerializeField] private CharacterType characterData;



    [Header("Ataques seleccionados")]

    [SerializeField] private int selectedMeleeIndex = 0;

    [SerializeField] private int selectedRangedIndex = 0;



    private Animator animator;

    private AudioSource audioSource;

    private PlayerInputHandler inputHandler;



    private IMeleeAttackStrategy meleeStrategy;

    private IRangedAttackStrategy rangedStrategy;



    public CharacterType CharacterData => characterData;

    public int SelectedMeleeIndex => selectedMeleeIndex;

    public int SelectedRangedIndex => selectedRangedIndex;

    public Animator Animator => animator;

    public AudioSource AudioSource => audioSource;

    public float LastAttackTime { get; set; }



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



    // CAMBIO MULTIJUGADOR: Pasamos de Update a FixedUpdateNetwork

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

}