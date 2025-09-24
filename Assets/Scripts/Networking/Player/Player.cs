using Fusion;
using Networking;
using System;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
public class Player : NetworkBehaviour
{
    private NetworkCharacterController _characterController;    

   private void Awake()
    {
            
        _characterController = GetComponent<NetworkCharacterController>();
     
    }
    public override void FixedUpdateNetwork()
    {

        if (GetInput(out NetworkInputPlayer inputPlayer))
        {

            inputPlayer.moveDirection.Normalize();
            _characterController.Move(inputPlayer.moveDirection * Runner.DeltaTime);

        }
   

    }

        
   
}
