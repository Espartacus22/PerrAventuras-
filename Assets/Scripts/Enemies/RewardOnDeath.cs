using UnityEngine;

public class RewardOnDeath : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyType enemyData;

    [Header("Optional")]
    public Transform dropSpawnPoint;

    private bool rewardsGiven = false;

    public void GiveRewards(PlayerLevel playerLevel)
    {
        if (rewardsGiven) return;
        if (playerLevel == null) return;
        if (enemyData == null) return;

        rewardsGiven = true;

        // XP
        if (enemyData.xpReward > 0)
            playerLevel.AddXP(enemyData.xpReward);

        // HP / Shield / Coins
        if (enemyData.fullRestoreOnKill)
        {
            playerLevel.RestoreFullState();
        }
        else
        {
            if (enemyData.hpReward > 0)
                playerLevel.Heal(enemyData.hpReward);

            if (enemyData.shieldReward > 0)
                playerLevel.RestoreShield(enemyData.shieldReward);
        }

        if (enemyData.coinReward > 0)
            playerLevel.AddCoins(enemyData.coinReward);

        // Drop visual opcional
        if (enemyData.dropPrefab != null)
        {
            Vector3 spawnPos = dropSpawnPoint != null ? dropSpawnPoint.position : transform.position;
            Instantiate(enemyData.dropPrefab, spawnPos, Quaternion.identity);
        }
    }
}
