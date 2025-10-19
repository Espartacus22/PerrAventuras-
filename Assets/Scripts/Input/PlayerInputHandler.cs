using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls controls;

    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool jump { get; private set; }
    public bool run { get; private set; }
    public bool crouch { get; private set; }
    public bool dash { get; private set; }


    private void Awake()
    {
        controls = new PlayerControls();

        // Moverse
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // Cámara
        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // Saltar
        controls.Player.Jump.performed += ctx => jump = true;
        controls.Player.Jump.canceled += ctx => jump = false;

        // Correr
        controls.Player.Run.performed += ctx => run = true;
        controls.Player.Run.canceled += ctx => run = false;

        // Agacharse
        controls.Player.Crouch.performed += ctx => crouch = true;
        controls.Player.Crouch.canceled += ctx => crouch = false;

        // Dash
        controls.Player.Dash.performed += ctx => dash = true;
        controls.Player.Dash.canceled += ctx => dash = false;
    }
    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

}
