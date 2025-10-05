using Fusion;
using UnityEngine;

namespace Networking

{
    public struct NetworkInputPlayer : INetworkInput
    {
        public const byte MOUSE_BUTTON_0 = 1;
        public const byte MOUSE_BUTTON_1 = 2;

        public NetworkButtons buttons; 
        public Vector3 moveDirection;
    }
}


