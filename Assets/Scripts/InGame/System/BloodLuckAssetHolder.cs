using InGame.Effects;
using InGame.Monsters;
using InGame.Projectiles;
using InGame.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace InGame.System
{
    public class BloodLuckAssetHolder : MonoBehaviour
    {
        [SerializeField] private BloodLuckMonsterComponent _monsterComponent;

        [SerializeField] private PerkSelectionViewComponent _perkSelectionViewPrefab;

        [SerializeField] private BloodLuckSectorAreaEffectComponent _sectorAreaEffectPrefab;

        [SerializeField] private BloodLuckSlashEffectComponent _slashEffectPrefab;

        [SerializeField] private Material _lightningLineMaterial;

        [SerializeField] private BloodLuckHudViewComponent _hudViewPrefab;

        [SerializeField] private BloodLuckProjectileComponent _projectilePrefab;

        [SerializeField] private SlotMachineViewComponent _slotMachineViewPrefab;

        [SerializeField] private BloodLuckGameResultViewComponent _gameResultViewPrefab;

        [SerializeField] private BloodLuck.BloodLuckShop _shopViewPrefab;

        [SerializeField] private PlayerAttack _playerAttackComponent;

        [SerializeField] private PlayerInputManager _playerInputManagerComponent;

        [SerializeField] private PlayerMovement _playerMovementComponent;

        [SerializeField] private CinemachineCamera _inGameCamera;

        public CinemachineCamera InGameCamera => _inGameCamera;
        public BloodLuckMonsterComponent GetBloodLuckMonsterComponent => _monsterComponent;
        public BloodLuckProjectileComponent ProjectilePrefab => _projectilePrefab;
        public Material LightningLineMaterial => _lightningLineMaterial;
        public BloodLuckHudViewComponent HudViewPrefab => _hudViewPrefab;
        public BloodLuckSectorAreaEffectComponent SectorAreaEffectPrefab => _sectorAreaEffectPrefab;
        public BloodLuckSlashEffectComponent SlashEffectPrefab => _slashEffectPrefab;
        public PerkSelectionViewComponent PerkSelectionViewPrefab => _perkSelectionViewPrefab;
        public SlotMachineViewComponent SlotMachineViewPrefab => _slotMachineViewPrefab;
        public BloodLuckGameResultViewComponent GameResultViewPrefab => _gameResultViewPrefab;
        public BloodLuck.BloodLuckShop ShopViewPrefab => _shopViewPrefab;
        public PlayerAttack GetPlayerAttackComponent => _playerAttackComponent;
        public PlayerInputManager GetPlayerInputManagerComponent => _playerInputManagerComponent;
        public PlayerMovement GetPlayerMovementComponent => _playerMovementComponent;
    }
}
