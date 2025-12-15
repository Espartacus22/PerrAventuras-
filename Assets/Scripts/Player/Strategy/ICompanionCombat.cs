using UnityEngine;

public interface ICompanionCombat
{
    void MeleeShort(Transform target);   // envestida / golpe corto
    void Ranged(Transform target);       // futuro
    void Block(bool enabled);            // escudo
}
