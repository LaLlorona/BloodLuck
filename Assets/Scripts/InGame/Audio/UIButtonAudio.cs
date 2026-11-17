using UnityEngine;
using UnityEngine.EventSystems;

namespace BloodLuck
{
    public sealed class UIButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            AudioManager.Instance?.PlayHover();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                AudioManager.Instance?.PlayUiSelect();
        }
    }
}
