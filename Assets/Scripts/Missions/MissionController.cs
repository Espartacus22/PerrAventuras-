using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class MissionController : NetworkBehaviour
{
    private MissionData _currentMission;
    public MissionData CurrentMission => _currentMission;

    public List<MissionStep> currentMissionSteps = new List<MissionStep>();
    public List<MissionStep> currentFailureSteps = new List<MissionStep>();

    public void StartNewMission(MissionData missionData)
    {
        if (_currentMission != null)
        {
            Destroy(_currentMission); 
        }

        // Clonamos el ScriptableObject en RAM de forma segura
        _currentMission = Instantiate(missionData);

        currentMissionSteps = _currentMission.missionSteps;
        currentFailureSteps = _currentMission.failureSteps;

        Debug.Log($"[Misiones] Nueva misión iniciada: {_currentMission.missionName}");
    }

    public void UpdateProgress(string id, int amount)
    {
        if (_currentMission == null) return;

        // Verificar progreso de pasos obligatorios de éxito
        var allComplete = true;
        foreach (var steps in currentMissionSteps)
        {
            steps.UpdateProgress(id, amount); 
            if (!steps.isComplete)
            {
                allComplete = false;
            }

            // Si un paso individual falla por su "failureId" interno
            if (steps.isFailed) 
            {
                FailMission();
                return;
            }
        }

        // 2. Verificar progreso de la lista global de fallos
        var allFailure = true;
        foreach (var steps in currentFailureSteps)
        {
            steps.UpdateProgress(id, amount); 
            if (!steps.isFailed)
            {
                allFailure = false;
            }
        }

        
        // RESOLUCIÓN DE LA MISIÓN

        if (allComplete)
        {
            CompleteMission();
        }
        else if (allFailure && currentFailureSteps.Count > 0)
        {
            FailMission();
        }
    }

    private void CompleteMission()
    {
        Debug.Log($"[Misiones] ¡Misión Completada!: {_currentMission.missionName}"); //
        Debug.Log($"[Recompensas] Otorgando Oro: {_currentMission.coins} y XP: {_currentMission.xp}"); //

        
        // PlayerStats.Instance.AddExperience(_currentMission.xp);

        CleanUpMission(); //
    }

    private void FailMission()
    {
        Debug.LogWarning($"[Misiones] La misión '{_currentMission.missionName}' ha FRACASADO.");
        CleanUpMission(); //
    }

    private void CleanUpMission()
    {
        if (_currentMission != null)
        {
            Destroy(_currentMission); // Borramos el clon de la RAM
            _currentMission = null;
        }
        currentMissionSteps.Clear();
        currentFailureSteps.Clear();
    }

    private void OnDestroy()
    {
        if (_currentMission != null)
        {
            Destroy(_currentMission);
        }
    }
}