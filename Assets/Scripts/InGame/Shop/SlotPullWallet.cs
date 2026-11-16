using UnityEngine;
using System;

namespace BloodLuck
{
    /// <summary>Standalone wallet for the Shop_Prototype scene only. In-game the player state manager is the wallet.</summary>
    public class SlotPullWallet : MonoBehaviour, IShopWallet
    {
        [SerializeField] private int startingPulls = 6;
        public int Balance { get; private set; }
        public event Action<int> BalanceChanged;

        private void Awake() => ResetForRun();

        public void ResetForRun()
        {
            Balance = startingPulls;
            BalanceChanged?.Invoke(Balance);
        }

        public bool CanSpend(int cost) => cost >= 0 && Balance >= cost;

        public bool TrySpend(int cost)
        {
            if (!CanSpend(cost)) return false;
            Balance -= cost;
            BalanceChanged?.Invoke(Balance);
            return true;
        }
    }
}
