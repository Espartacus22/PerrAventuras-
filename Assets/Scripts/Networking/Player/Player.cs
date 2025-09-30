using Fusion;
using Networking;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
public class Player : NetworkBehaviour
{
    private NetworkCharacterController _characterController;
    [SerializeField] private Renderer _renderer;
    [SerializeField]private NetworkObject _projectilePrefab;
    [SerializeField] private Transform _projectileSpawnPoint;

    private void Awake()
    {
            
        _characterController = GetComponent<NetworkCharacterController>();
     
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
        
         _renderer.material.color = Color.yellow;             
        
        
        }
        
     

        }
    public override void FixedUpdateNetwork()
    {

        if (!GetInput(out NetworkInputPlayer inputPlayer)) return;
        {

            inputPlayer.moveDirection.Normalize();
            _characterController.Move(inputPlayer.moveDirection * Runner.DeltaTime);
            
            if (inputPlayer.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_0)) 
            {
                Runner.Spawn(_projectilePrefab, _projectileSpawnPoint.position, Quaternion.LookRotation(transform.forward), Object.InputAuthority);
            }
  
    }

    }

}
