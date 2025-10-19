using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 GetMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        return new Vector2(x, y);
    }

    public bool GetJump() => Input.GetButtonDown("Jump");
    public bool GetDash() => Input.GetKeyDown(KeyCode.LeftShift);
    public bool GetAttack() => Input.GetMouseButtonDown(0);
}