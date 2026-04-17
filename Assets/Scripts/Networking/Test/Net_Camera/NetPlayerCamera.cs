using Fusion;
using Networking;
using UnityEngine;

namespace Networking
{
    public class NetPlayerCamera : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AudioListener audioListenerComp;

        [Header("Follow")]
        [SerializeField] private Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f);

        [Header("Rotation")]
        [SerializeField] private float mouseSensitivity = 2.5f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 60f;

        private float yaw;
        private float pitch;

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                if (playerCamera != null)
                    playerCamera.gameObject.SetActive(true);

                if (audioListenerComp != null)
                    audioListenerComp.enabled = true;

                Vector3 startEuler = cameraPivot.rotation.eulerAngles;
                yaw = startEuler.y;
                pitch = cameraPivot.localEulerAngles.x;

                if (pitch > 180f)
                    pitch -= 360f;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                if (playerCamera != null)
                    playerCamera.gameObject.SetActive(false);

                if (audioListenerComp != null)
                    audioListenerComp.enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (!Object || !Object.HasInputAuthority)
                return;

            // Mouse
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * mouseSensitivity;
            pitch -= mouseY * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            // El pivot sigue al player
            if (cameraPivot != null)
            {
                cameraPivot.position = transform.position + pivotOffset;
                cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
        }

        public Vector3 GetCameraPlanarForward()
        {
            if (cameraPivot == null)
                return transform.forward;

            Vector3 forward = cameraPivot.forward;
            forward.y = 0f;
            return forward.normalized;
        }

        public Vector3 GetCameraPlanarRight()
        {
            if (cameraPivot == null)
                return transform.right;

            Vector3 right = cameraPivot.right;
            right.y = 0f;
            return right.normalized;
        }
    }
}
