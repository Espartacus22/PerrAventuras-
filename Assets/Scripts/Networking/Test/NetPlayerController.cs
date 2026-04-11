using Fusion;
using UnityEngine;
using Networking;

public class NetPlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<NetInputData>(out var inputData))
            return;

        Vector3 move = new Vector3(inputData.move.x, 0f, inputData.move.y);

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Runner.DeltaTime;
        }

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Runner.DeltaTime);

        if (move != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(move);
        }
    }
}
