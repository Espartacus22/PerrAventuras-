using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    // Todas las propiedades ahora tienen 'set' público para que Fusion las manipule
    public Vector2 MoveInput { get; set; }
    public bool JumpPressed { get; set; }
    public bool RunHeld { get; set; }
    public bool DashPressed { get; set; }
    public bool CrouchPressed { get; set; }
    public bool CrouchReleased { get; set; }
    public bool MeleePressed { get; set; }
    public bool RangedPressed { get; set; }
}
