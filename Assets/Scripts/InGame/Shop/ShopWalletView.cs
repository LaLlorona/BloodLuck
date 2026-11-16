using TMPro;
using UnityEngine;

namespace BloodLuck
{
    /// <summary>Displays the shop wallet balance wherever the shop UI needs it.</summary>
    public class ShopWalletView : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private string labelPrefix = "HP: ";
        private IShopWallet wallet;

        public void Bind(IShopWallet source)
        {
            if (wallet != null) wallet.BalanceChanged -= Refresh;
            wallet = source;
            if (wallet != null) wallet.BalanceChanged += Refresh;
            Refresh(wallet == null ? 0 : wallet.Balance);
        }

        private void OnDestroy()
        {
            if (wallet != null) wallet.BalanceChanged -= Refresh;
        }

        private void Refresh(int balance)
        {
            if (label != null) label.text = labelPrefix + balance;
        }
    }
}
