using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Configuración de teclas")]
    private KeyCode dashKey = KeyCode.LeftShift;
    private KeyCode crouchKey = KeyCode.LeftControl;
    private KeyCode meleeAttackKey = KeyCode.Mouse0;
    private KeyCode rangedAttackKey = KeyCode.Mouse1;
    private KeyCode runKey = KeyCode.LeftAlt;

    public Vector2 moveInput;
    public bool jumpPressed;
    public bool dashPressed;
    public bool crouchHeld;
    public bool meleeAttackPressed;
    public bool rangedAttackPressed;
    public bool runHeld;

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

    // Métodos para acceder desde PlayerLocal
    public Vector2 GetMovement() => moveInput;
    public bool GetJump() => jumpPressed;
    public bool GetDash() => dashPressed;
    public bool GetCrouch() => crouchHeld;
    public bool GetRun() => runHeld;
    public bool GetAttackMelee() => meleeAttackPressed;
    public bool GetAttackRanged() => rangedAttackPressed;
}