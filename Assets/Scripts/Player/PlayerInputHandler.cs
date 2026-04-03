using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool RunHeld { get; private set; }
    public bool DashPressed { get; private set; }
    public bool CrouchPressed { get; private set; }
    public bool CrouchReleased { get; private set; }

    public bool MeleePressed { get; private set; }
    public bool RangedPressed { get; private set; }

    private void Update()
    {
        MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        JumpPressed = Input.GetButtonDown("Jump");
        RunHeld = Input.GetKey(KeyCode.LeftShift);
        DashPressed = Input.GetKeyDown(KeyCode.LeftAlt);

        CrouchPressed = Input.GetKeyDown(KeyCode.LeftControl);
        CrouchReleased = Input.GetKeyUp(KeyCode.LeftControl);

        MeleePressed = Input.GetMouseButtonDown(0);
        RangedPressed = Input.GetMouseButtonDown(1);

        if (MeleePressed) Debug.Log("InputHandler: click izquierdo");
        if (RangedPressed) Debug.Log("InputHandler: click derecho");
    }
}
