using InGame.Effects;
using InGame.Monsters;
using InGame.Projectiles;
using InGame.UI;
using UnityEngine;

namespace InGame.System
{
    public class BloodLuckAssetHolder : MonoBehaviour
    {
        [SerializeField] private BloodLuckMonsterComponent _monsterComponent;

        [SerializeField] private PerkSelectionViewComponent _perkSelectionViewPrefab;

        [SerializeField] private BloodLuckSectorAreaEffectComponent _sectorAreaEffectPrefab;

        [SerializeField] private BloodLuckHudViewComponent _hudViewPrefab;

        [SerializeField] private BloodLuckProjectileComponent _projectilePrefab;

        public BloodLuckMonsterComponent GetBloodLuckMonsterComponent => _monsterComponent;
        public BloodLuckProjectileComponent ProjectilePrefab => _projectilePrefab;
        public BloodLuckHudViewComponent HudViewPrefab => _hudViewPrefab;
        public BloodLuckSectorAreaEffectComponent SectorAreaEffectPrefab => _sectorAreaEffectPrefab;
        public PerkSelectionViewComponent PerkSelectionViewPrefab => _perkSelectionViewPrefab;
    }
}
