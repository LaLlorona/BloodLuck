using UnityEngine;
using UnityEngine.EventSystems;

namespace BloodLuck
{
    public sealed class ShopCardHoverSound : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            AudioManager.Instance?.PlayHover();
        }
    }
}
