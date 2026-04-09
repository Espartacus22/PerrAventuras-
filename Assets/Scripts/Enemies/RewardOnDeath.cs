using UnityEngine;

public class RewardOnDeath : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyType enemyData;

    private bool rewardsGiven = false;

    public void GiveRewards(PlayerLevel playerLevel)
    {
        if (rewardsGiven) return;
        if (playerLevel == null) return;
        if (enemyData == null) return;

        rewardsGiven = true;

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
    }
}
