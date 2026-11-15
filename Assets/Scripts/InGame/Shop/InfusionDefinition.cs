using Player;
using UnityEngine;

namespace BloodLuck
{
    public enum InfusionRarity { Bloodstained, Bloodbound, Bloodwrought }

    public enum InfusionPassiveKind { Special, MaxHp, Damage, AttackSpeed }

    [CreateAssetMenu(menuName = "BloodLuck/Infusion")]
    public class InfusionDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public InfusionRarity rarity;

        [Header("Player Passive")]
        public InfusionPassiveKind passiveKind = InfusionPassiveKind.Special;
        public OperationMethod operationMethod = OperationMethod.Add;
        public float passiveValue;

        public int Cost => rarity == InfusionRarity.Bloodwrought ? 3 : 1;
        public bool Stackable => rarity == InfusionRarity.Bloodstained;

        /// <summary>Creates the passive applied to the player when this infusion is purchased.</summary>
        public PlayerPassive CreatePassive() => passiveKind switch
        {
            InfusionPassiveKind.MaxHp => new PlayerPassive.IncreaseMaxHp(passiveValue, operationMethod),
            InfusionPassiveKind.Damage => new PlayerPassive.IncreasePlayerDamage(passiveValue, operationMethod),
            InfusionPassiveKind.AttackSpeed => new PlayerPassive.AttackSpeedUp(passiveValue, operationMethod),
            _ => new PlayerPassive.SPECIAL_ABILITIES_HAHA()
        };
    }
}
