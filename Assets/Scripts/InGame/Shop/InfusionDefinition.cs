using UnityEngine;

namespace BloodLuck
{
    public enum InfusionRarity { Bloodstained, Bloodbound, Bloodwrought }

    [CreateAssetMenu(menuName = "BloodLuck/Infusion")]
    public class InfusionDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public InfusionRarity rarity;

        public int Cost => rarity == InfusionRarity.Bloodwrought ? 3 : 1;
        public bool Stackable => rarity == InfusionRarity.Bloodstained;
    }
}
