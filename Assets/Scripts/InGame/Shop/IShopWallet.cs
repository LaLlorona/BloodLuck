using System;

namespace BloodLuck
{
    /// <summary>Pull balance used by the shop. In-game this is BloodLuckPlayerStateManager.</summary>
    public interface IShopWallet
    {
        int Pulls { get; }
        event Action<int> PullsChanged;
        bool TrySpend(int cost);
    }
}
