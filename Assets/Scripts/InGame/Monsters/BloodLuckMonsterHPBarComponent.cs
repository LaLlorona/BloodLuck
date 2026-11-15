using UnityEngine;

namespace InGame.Monsters
{
    public class BloodLuckMonsterHPBarComponent : MonoBehaviour
    {
        [Tooltip("Stretched child of the hp bar background. Its right anchor is scaled by the hp ratio.")]
        [SerializeField] private RectTransform _hpFill;

        public void SetHp(float currentHp, float maxHp)
        {
            float ratio = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;
            _hpFill.anchorMax = new Vector2(ratio, 1f);
        }
    }
}
