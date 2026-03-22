using UnityEngine;
using Fusion;
using Networking;
using System.Collections;

public class PlayerHealth : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnHealthChanged))] public int health { get; set; } = 100;

    [SerializeField] private MeshRenderer _renderer;
    private ChangeDetector _changeDetector;
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }
    public override void FixedUpdateNetwork()
    {
      if (GetInput(out NetworkInputPlayer inputPlayer))
        { 
            if (inputPlayer.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_1))
            {

                RPC_RequestDamage(10);
            }
        }
    }

    public override void Render()
    {
        foreach (var changeVariable in _changeDetector.DetectChanges(this))

        {

            if (changeVariable == nameof(health))
                {
                Debug.Log($"Health changed to {health}");

            }
        }
    }


    //Server Logic
    private void ServerTakeDamage (int damage)
    {
        Debug.Log($"Player {name} takes damage {damage}, hasAuth: {HasStateAuthority}");
        if (!HasStateAuthority) return;

        health -= Mathf.Max(0,health - damage);

        Debug.Log($"Player {name} has {health} health");
    }

    private void OnHealthChanged ()
    { 
        Debug.Log($"Has State Auth: [{HasStateAuthority}]. HasInputAuth: {HasInputAuthority}  OnHealthChanged: {health}");   
    }

    //Client (InputAuthority) ---> Server

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable, HostMode =RpcHostMode.SourceIsServer, InvokeLocal = true, TickAligned = true)] //el 3er argumento en false si no queremos que se invoque de manera local
    private void RPC_RequestDamage(int damage, RpcInfo info = default)
    {
        if (!HasStateAuthority) return; 
        ServerTakeDamage (damage);

        RPC_HitDamageFeedback();

    }


    //Server --> Clients (All)

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Unreliable, TickAligned = false)]
    private void RPC_HitDamageFeedback(RpcInfo info = default)
    {
        StartCoroutine (routine:DamageFeedback());

    }

    private IEnumerator DamageFeedback ()
    {
        var initialColor = _renderer.material.color;
        _renderer.material.color = Color.white;
        yield return new WaitForSeconds(1f);
        _renderer.material.color = initialColor;    
    }
}

