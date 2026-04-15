using Unity.Cinemachine;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public CinemachineCamera gameplayCamera;
    public CinemachineCamera dialogueCamera;

    public void EnterDialogue()
    {
        if (gameplayCamera != null) gameplayCamera.Priority = 10;
        if (dialogueCamera != null) dialogueCamera.Priority = 20;
    }

    public void ExitDialogue()
    {
        if (dialogueCamera != null) dialogueCamera.Priority = 0;
        if (gameplayCamera != null) gameplayCamera.Priority = 10;
    }
}
