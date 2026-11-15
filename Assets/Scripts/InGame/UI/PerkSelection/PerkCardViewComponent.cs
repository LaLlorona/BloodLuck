using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{
    /// <summary>선택창의 카드 1개. 왼쪽 스프라이트 / 오른쪽 텍스트, 클릭 가능한 Button.</summary>
    public class PerkCardViewComponent : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;

        public Button Button => _button;

        public void Setup(PerkData data, Action onClick)
        {
            _icon.sprite = data.Icon;
            _icon.enabled = data.Icon != null;
            _title.text = data.Passive.GetTitle();
            _description.text = data.Passive.GetDescription();

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
