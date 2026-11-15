using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace InGame.UI
{
    public class PerkSelectionView : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _introDirector;
        [SerializeField] private PerkCardView[] _cards;
   

        private bool _selected;

        public event Action<PerkData> _perkSelected;
        
        public void Show(IReadOnlyList<PerkData> perks, Action<PerkData> onPerkSelected)
        {
            _selected = false;
            _perkSelected = onPerkSelected;

            for (int i = 0; i < _cards.Length; i++)
            {
                bool hasPerk = i < perks.Count;
                _cards[i].gameObject.SetActive(hasPerk);
                if (!hasPerk) continue;

                PerkData perk = perks[i];
                _cards[i].Setup(perk, () => OnCardClicked(perk));
            }
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnCardClicked(PerkData perk)
        {
            if (_selected) return;
            _selected = true;
            _perkSelected?.Invoke(perk);
        }
    }
}
