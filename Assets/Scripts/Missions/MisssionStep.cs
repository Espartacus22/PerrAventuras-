using System;
using UnityEngine;

[System.Serializable]
public class MissionStep
{
    public string targetId;    // Ej: Gain_XP, Pick_Item
    public int amount;         // Meta total
    public string failureId;   // ID de fracaso

    [NonSerialized] public bool isComplete; //
    [NonSerialized] public int currentAmount; //
    [NonSerialized] public bool isFailed; //


    public void UpdateProgress(string id, int amount)
    {
        if (isComplete || isFailed) return; //

        // Comprobación de fracaso
        if (string.CompareOrdinal(id, failureId) == 0) //
        {
            isFailed = true; //
            return; //
        }

        // Comprobación de éxito
        if (string.CompareOrdinal(id, targetId) != 0) return; //

        currentAmount += amount; //

        if (currentAmount >= this.amount) //
        {
            currentAmount = this.amount; // Anti desbordamiento
            isComplete = true; //
        }
    }
}