namespace InGame.System
{
    /// <summary>Lower values are updated first.</summary>
    public enum BloodLuckUpdatePriority
    {
        CheckAspectRatioUpdate = -3,
        InputUpdate = -2,
        PlayerWeaponUpdate = -1,
        PlayerBuffUpdate = 0,
        ProjectileUpdate = 1,
        AoEUpdate = 2,
        MonsterUpdate = 3,
        CharacterSkillStateUpdate = 5,
        HUDUpdate = 6,
        DamageTagUpdate = 7,
        EffectUpdate = 100,
    }

    public interface IBloodLuckPriorityUpdatable
    {
        BloodLuckUpdatePriority UpdatePriority { get; }

        void GameUpdate(float deltaTime);
    }
}
