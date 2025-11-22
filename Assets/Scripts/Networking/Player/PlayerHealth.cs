using UnityEngine;
using UnityEngine.EventSystems; // para detectar si el mouse está sobre la UI del inventario
using Fusion;
using Networking;
using System.Collections;

public class PlayerHealth : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public int health { get; set; } = 100;

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
            // Al presionar el boton derecho del mouse el jugador pierde 10 de vida a los fines de testear el uso de consumibles y que se le reste la misma.
            if (HasInputAuthority &&
                inputPlayer.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_1) &&
                !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
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

    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestDamage(int damage)
    {
        if (!Object.HasStateAuthority) return;
        health -= damage;

      
        if (health < 0) health = 0;

        // feedback visualde daño todos los clientes
        RPC_HitDamageFeedback();
    }

    //CURACIÓN con consumibles
    
    public void Heal(int amount)
    {
        // Solo el server puede modificar la vida
        if (!Object.HasStateAuthority) return;

        health += amount;

        // feedback visual de curación (verde)
        RPC_HealFeedback();
    }

   
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HitDamageFeedback()
    {
        StartCoroutine(DamageFeedback());
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HealFeedback()
    {
        StartCoroutine(HealFeedback());
    }

    private IEnumerator DamageFeedback()
    {
        var initialColor = _renderer.material.color;
        _renderer.material.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        _renderer.material.color = initialColor;
    }

    private IEnumerator HealFeedback()
    {
        var initialColor = _renderer.material.color;
        _renderer.material.color = Color.green;
        yield return new WaitForSeconds(0.3f);
        _renderer.material.color = initialColor;
    }

    private void OnHealthChanged()
    {
        Debug.Log($"Health: {health} | StateAuth: {HasStateAuthority} | InputAuth: {HasInputAuthority}");
    }
}