using Fusion;
using Networking;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

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
   
                // Conectar la UI al inventario de este jugador
                InventoryUI.I.AttachInventory(GetComponent<InventorySystem>());
         


            _renderer.material.color = Color.yellow;             
        
        
        }
        
     
        }
    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputPlayer inputPlayer)) return;

        inputPlayer.moveDirection.Normalize();
        _characterController.Move(inputPlayer.moveDirection * Runner.DeltaTime * 5f); 

        if (inputPlayer.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_0) &&
     HasInputAuthority &&
     !EventSystem.current.IsPointerOverGameObject())
        {
            RPC_RequestFire(_projectileSpawnPoint.position, transform.forward);
        }

    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestFire(Vector3 position, Vector3 direction)
    {
        
        var proj = Runner.Spawn(_projectilePrefab, position, Quaternion.LookRotation(direction), Object.InputAuthority);
        proj.GetComponent<Projectile>().InitProjectile();
    }
}
