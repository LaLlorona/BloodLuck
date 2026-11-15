using BloodLuck;
using Cysharp.Threading.Tasks;
using InGame.System;
using Player;
using UnityEngine;

namespace InGame.Managers
{
    public class BloodLuckPlayerStateManager : IBloodLuckContext, IShopWallet
    {
        public float MaxHp => _passiveContainer.GetMaxHp(_parameter.PlayerMaxHp);
        public float CurrentHp { get; private set; }
        public int MaxPulls { get; }
        public int CurrentPulls { get; private set; }
        public event global::System.Action<int> PullsChanged;
        public float Attack => _passiveContainer.GetDamage(_parameter.PlayerAttack);
        public float AttackSpeed => _passiveContainer.GetAttackSpeed(_parameter.PlayerAttackSpeed);

        private readonly BloodLuckInGameParameter _parameter;
        private readonly PlayerPassiveContainer _passiveContainer = new();
        private BloodLuckUIManager _uiManager;

        public BloodLuckPlayerStateManager(BloodLuckInGameParameter parameter)
        {
            _parameter = parameter;
            CurrentHp = MaxHp;
            MaxPulls = parameter.PlayerMaxPulls;
            CurrentPulls = MaxPulls;
        }

        public void AddPassive(PlayerPassive passive)
        {
            float previousMaxHp = MaxHp;
            _passiveContainer.AddPassive(passive);
            ApplyMaxHpChange(previousMaxHp);
        }

        public void RemovePassive(PlayerPassive passive)
        {
            float previousMaxHp = MaxHp;
            if (_passiveContainer.RemovePassive(passive))
            {
                ApplyMaxHpChange(previousMaxHp);
            }
        }
        
        private void ApplyMaxHpChange(float previousMaxHp)
        {
            float delta = MaxHp - previousMaxHp;
            if (Mathf.Approximately(delta, 0f))
            {
                return;
            }

            float hp = CurrentHp + delta;
            if (delta < 0f)
            {
                hp = Mathf.Max(hp, 1f);
            }

            CurrentHp = Mathf.Clamp(hp, 0f, MaxHp);
            _uiManager.RefreshHud();
        }

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _uiManager = contextMap.GetRegisteredInstance<BloodLuckUIManager>();
        }

        public void TakeDamage(float amount)
        {
            SetCurrentHp(CurrentHp - amount);
        }

        public void Heal(float amount)
        {
            SetCurrentHp(CurrentHp + amount);
        }

        public void SetCurrentPulls(int pulls)
        {
            int clamped = Mathf.Clamp(pulls, 0, MaxPulls);
            if (clamped == CurrentPulls)
            {
                return;
            }

            CurrentPulls = clamped;
            _uiManager.RefreshHud();
            PullsChanged?.Invoke(CurrentPulls);
        }

        public bool TrySpendPulls(int cost)
        {
            if (cost < 0 || CurrentPulls < cost)
            {
                return false;
            }

            SetCurrentPulls(CurrentPulls - cost);
            return true;
        }

        int IShopWallet.Pulls => CurrentPulls;

        bool IShopWallet.TrySpend(int cost) => TrySpendPulls(cost);

        private void SetCurrentHp(float hp)
        {
            float clamped = Mathf.Clamp(hp, 0f, MaxHp);
            if (Mathf.Approximately(clamped, CurrentHp))
            {
                return;
            }

            CurrentHp = clamped;
            _uiManager.RefreshHud();
        }
    }
}
