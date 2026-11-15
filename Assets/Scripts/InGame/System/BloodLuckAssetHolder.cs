using InGame.Monsters;
using InGame.UI;
using UnityEngine;

namespace InGame.System
{
    public class BloodLuckAssetHolder : MonoBehaviour
    {
        [SerializeField] private BloodLuckMonsterComponent _monsterComponent;

        // Prefab asset (not a scene object). It is instantiated when needed and destroyed afterwards.
        [SerializeField] private PerkSelectionView _perkSelectionViewPrefab;

        public BloodLuckMonsterComponent GetBloodLuckMonsterComponent => _monsterComponent;
        public PerkSelectionView PerkSelectionViewPrefab => _perkSelectionViewPrefab;
    }
}
