using UnityEngine;

[CreateAssetMenu(fileName = "CharacterType", menuName = "Scriptable Objects/CharacterType")]
public class CharacterType : ScriptableObject
{
    [Header("Identidad")]
    public string characterName = "Piki";
    public float hp = 100f;

    [Header("Movimiento")]
    public float walkSpeed = 3f;
    public float runMultiplier = 1.5f;
    public float rotationSpeed = 10f;
    public float crouchMultiplier = 0.5f;

    [Header("Salto")]
    public float jumpForce = 7f;
    public bool dobleSalto = true;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;

    [Header("Agacharse")]
    public float crouchHeight = 0.5f;

    [Header("Ataques cuerpo a cuerpo")]
    public int meleeAttacks = 1;

    [Header("Ataques a distancia")]
    public int rangedAttacks = 1;
}