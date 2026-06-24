using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "Scriptable Objects/MissionData")]
public class MissionData : ScriptableObject
{
    [Header("Información General")]
    public string missionId;   // Identificador global de la mision
    public string missionName; // Nombre de la mision
    [TextArea] public string description; // Descripcion de la Mision

    [Header("Flujo de la Misión")]
    public List<MissionStep> missionSteps; // Pasos para GANAR
    public List<MissionStep> failureSteps; // Pasos que hacen PERDER

    [Header("Recompensas")] 
    public int xp;
    public int coins;
}








