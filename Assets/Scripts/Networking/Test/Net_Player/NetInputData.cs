using Fusion;
using Networking;
using UnityEngine;

namespace Networking
{
    public struct NetInputData : INetworkInput
    {
        public const byte MOUSE_LEFT = 0;
        public const byte MOUSE_RIGHT = 1;
        public const byte JUMP = 2;
        public const byte DASH = 3;
        public const byte RUN = 4;

        public NetworkButtons buttons;
        public Vector2 move;

        // Helpers
        public Vector2 moveInput => move;

        public bool isRunning => buttons.IsSet(RUN);
        public bool wasJumpPressed => buttons.IsSet(JUMP);
        public bool wasDashPressed => buttons.IsSet(DASH);
        public bool wasMouseLeftPressed => buttons.IsSet(MOUSE_LEFT);
        public bool wasMouseRightPressed => buttons.IsSet(MOUSE_RIGHT);
    }
}
