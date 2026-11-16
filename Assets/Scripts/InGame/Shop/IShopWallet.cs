using System;

namespace BloodLuck
{
    /// <summary>Currency balance used by the shop. In-game this is BloodLuckPlayerStateManager, spending current HP.</summary>
    public interface IShopWallet
    {
        int Balance { get; }
        event Action<int> BalanceChanged;
        bool CanSpend(int cost);
        bool TrySpend(int cost);
    }
}
