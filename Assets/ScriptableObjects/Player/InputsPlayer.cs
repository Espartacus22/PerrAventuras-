using UnityEngine;

[CreateAssetMenu(fileName = "InputsPlayer", menuName = "Scriptable Objects/InputsPlayer")]
public class InputsPlayer : ScriptableObject
{
    [Header("Teclas de movimiento")]
    public KeyCode runKey = KeyCode.LeftAlt;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode dashKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
}
