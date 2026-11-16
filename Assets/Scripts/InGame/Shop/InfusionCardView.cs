using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BloodLuck
{
    /// <summary>Small presentation component for one shop offer. It owns no purchase logic.</summary>
    public class InfusionCardView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text rarity;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_Text cost;
        [SerializeField] private GameObject contents;
        [SerializeField] private Image rarityTint;
        [SerializeField] private RarityStyle stained = new RarityStyle(new Color(0.62f, 0.57f, 0.47f), new Color(0f, 0f, 0f, 0f));
        [SerializeField] private RarityStyle bound = new RarityStyle(new Color(0.9f, 0.25f, 0.2f), new Color(0.55f, 0.04f, 0.04f, 0.4f));
        [SerializeField] private RarityStyle wrought = new RarityStyle(new Color(1f, 0f, 0f), new Color(0.85f, 0f, 0f, 0.65f));

        [System.Serializable]
        private struct RarityStyle
        {
            public Color labelColor;
            public Color tintColor;

            public RarityStyle(Color labelColor, Color tintColor)
            {
                this.labelColor = labelColor;
                this.tintColor = tintColor;
            }
        }

        public void SetInfusion(InfusionDefinition infusion, int hpCost)
        {
            bool hasInfusion = infusion != null;
            if (contents != null) contents.SetActive(hasInfusion);
            if (rarityTint != null) rarityTint.enabled = hasInfusion;
            if (!hasInfusion) return;

            ApplyRarityStyle(infusion.rarity);

            if (icon != null)
            {
                icon.sprite = infusion.icon;
                icon.enabled = infusion.icon != null;
            }
            if (title != null) title.text = infusion.displayName;
            if (rarity != null) rarity.text = RarityLabel(infusion.rarity);
            if (description != null) description.text = infusion.description;
            if (cost != null) cost.text = hpCost + " HP";
        }

        private void ApplyRarityStyle(InfusionRarity value)
        {
            RarityStyle style = value switch
            {
                InfusionRarity.Bloodstained => stained,
                InfusionRarity.Bloodbound => bound,
                _ => wrought
            };
            if (rarity != null) rarity.color = style.labelColor;
            if (rarityTint != null) rarityTint.color = style.tintColor;
        }

        private static string RarityLabel(InfusionRarity value) => value switch
        {
            InfusionRarity.Bloodstained => "BLOODSTAINED",
            InfusionRarity.Bloodbound => "BLOODBOUND",
            _ => "BLOODWROUGHT"
        };
    }
}
