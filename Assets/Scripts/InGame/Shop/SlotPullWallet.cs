using UnityEngine;
using System;

namespace BloodLuck
{
    public class SlotPullWallet : MonoBehaviour
    {
        [SerializeField] private int startingPulls = 6;
        public int Pulls { get; private set; }
        public event Action<int> PullsChanged;

        private void Awake() => ResetForRun();

        public void ResetForRun()
        {
            Pulls = startingPulls;
            PullsChanged?.Invoke(Pulls);
        }

        public bool TrySpend(int cost)
        {
            if (cost < 0 || Pulls < cost) return false;
            Pulls -= cost;
            PullsChanged?.Invoke(Pulls);
            return true;
        }
    }
}
