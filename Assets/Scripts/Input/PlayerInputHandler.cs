using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Configuración de teclas")]
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode meleeAttackKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode rangedAttackKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode runKey = KeyCode.LeftAlt;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool dashPressed;
    private bool crouchHeld;
    private bool meleeAttackPressed;
    private bool rangedAttackPressed;
    private bool runHeld;

    void Update()
    {
        // Movimiento base
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Saltar
        jumpPressed = Input.GetButtonDown("Jump");

        // Correr
        runHeld = Input.GetKey(runKey);

        // Dash
        dashPressed = Input.GetKeyDown(dashKey);

        // Agacharse
        crouchHeld = Input.GetKey(crouchKey);

        // Ataques
        meleeAttackPressed = Input.GetKeyDown(meleeAttackKey);
        rangedAttackPressed = Input.GetKeyDown(rangedAttackKey);
    }

    // Métodos públicos para acceder desde PlayerLocal
    public Vector2 GetMovement() => moveInput;
    public bool GetJump() => jumpPressed;
    public bool GetDash() => dashPressed;
    public bool GetCrouch() => crouchHeld;
    public bool GetRun() => runHeld;
    public bool GetAttackMelee() => meleeAttackPressed;
    public bool GetAttackRanged() => rangedAttackPressed;
}