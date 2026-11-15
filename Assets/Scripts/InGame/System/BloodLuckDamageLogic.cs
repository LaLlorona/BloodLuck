using InGame.Managers;

namespace InGame.System
{
    public static class BloodLuckDamageLogic
    {
        public static void ApplyDamageToPlayer(BloodLuckPlayerStateManager playerStateManager, float damage)
        {
            playerStateManager.TakeDamage(damage);
        }

        public static void ApplyDamageToMonster(BloodLuckPlayerStateManager playerStateManager, float damage)
        {
            
        }
    }
}
