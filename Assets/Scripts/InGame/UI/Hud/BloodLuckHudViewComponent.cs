using TMPro;
using UnityEngine;

namespace InGame.UI
{
    public class BloodLuckHudViewComponent : MonoBehaviour
    {
        [Tooltip("Stretched child of the hp bar background. Its right anchor is scaled by the hp ratio.")]
        [SerializeField] private RectTransform _hpFill;
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _pullsText;

        public void SetHp(float currentHp, float maxHp)
        {
            float ratio = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;
            _hpFill.anchorMax = new Vector2(ratio, 1f);
            _hpText.text = $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)}";
        }

        public void SetPulls(int currentPulls, int maxPulls)
        {
            _pullsText.text = $"{currentPulls}/{maxPulls}";
        }
    }
}
