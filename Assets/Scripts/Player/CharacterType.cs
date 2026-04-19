using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterType", menuName = "Scriptable Objects/CharacterType")]
public class CharacterType : ScriptableObject
{
    [Header("Identidad")]
    public string characterName;
    public int hp;

    [Header("Movimiento")]
    public float walkSpeed = 5f;
    public float runMultiplier = 1.5f;
    public float crouchMultiplier = 0.5f;
    public float rotationSpeed = 10f;

    [Header("Salto")]
    public float jumpForce = 8f;
    public bool dobleSalto = false;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;

    [Header("Agacharse")]
    public float crouchHeight = 0.5f;

    [Header("Ataques Cuerpo a Cuerpo")]
    public List<MeleeAttackData> meleeAttacks;

    [Header("Ataques a Distancia")]
    public List<RangedAttackData> rangedAttacks;
}