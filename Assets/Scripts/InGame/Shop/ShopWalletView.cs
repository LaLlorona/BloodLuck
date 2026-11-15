using TMPro;
using UnityEngine;

namespace BloodLuck
{
    /// <summary>Displays the shared slot-pull wallet wherever the shop UI needs it.</summary>
    public class ShopWalletView : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        private SlotPullWallet wallet;

        public void Bind(SlotPullWallet source)
        {
            if (wallet != null) wallet.PullsChanged -= Refresh;
            wallet = source;
            if (wallet != null) wallet.PullsChanged += Refresh;
            Refresh(wallet == null ? 0 : wallet.Pulls);
        }

        private void OnDestroy()
        {
            if (wallet != null) wallet.PullsChanged -= Refresh;
        }

        private void Refresh(int pulls)
        {
            if (label != null) label.text = "PULLS: " + pulls;
        }
    }
}
