using Fusion;
using UnityEngine;

namespace Networking
{
    public struct NetworkInputPlayer : INetworkInput
    {
        public const byte MOUSE_BUTTON_0 = 1;
        public const byte MOUSE_BUTTON_1 = 2;
        public const byte JUMP = 3;
        public const byte DASH = 4;
        public const byte CROUCH = 5;
        public const byte RUN = 6;

        public NetworkButtons buttons;
        public Vector2 moveInput;      // Para el WASD
        public Vector3 lookDirection;  // Para saber a dónde mira el jugador
    }
}


