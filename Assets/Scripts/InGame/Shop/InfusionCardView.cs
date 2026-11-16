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

        public void SetInfusion(InfusionDefinition infusion, int hpCost)
        {
            bool hasInfusion = infusion != null;
            if (contents != null) contents.SetActive(hasInfusion);
            if (!hasInfusion) return;

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

        private static string RarityLabel(InfusionRarity value) => value switch
        {
            InfusionRarity.Bloodstained => "BLOODSTAINED",
            InfusionRarity.Bloodbound => "BLOODBOUND",
            _ => "BLOODWROUGHT"
        };
    }
}
