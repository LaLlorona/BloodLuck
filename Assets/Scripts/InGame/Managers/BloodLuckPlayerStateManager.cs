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
        public event global::System.Action<int> BalanceChanged;
        public float Attack => _passiveContainer.GetDamage(_parameter.PlayerAttack);
        public float ThirdAttack => _passiveContainer.GetThirdAttackDamage(_parameter.PlayerThirdAttackDamage);
        public float ChainLightningDamage => _passiveContainer.GetChainLightningDamage();
        public float DashDamage => _passiveContainer.GetDashDamage();
        public float LifeStealRatio => _passiveContainer.GetLifeStealRatio();
        public float AttackSpeed => _passiveContainer.GetAttackSpeed(_parameter.PlayerAttackSpeed);

        private readonly BloodLuckInGameParameter _parameter;
        private readonly PlayerPassiveContainer _passiveContainer = new();
        private BloodLuckUIManager _uiManager;
        private BloodLuckGameStateManager _gameStateManager;
        private BloodLuckGameResultManager _gameResultManager;
        private bool _deathSoundPlayed;
        public bool IsGameProceeding => _gameStateManager.CurrentGameState == new BloodLuckGameStateManager.GameState.GameProceeding();

        public BloodLuckPlayerStateManager(BloodLuckManagerParameter parameter)
        {
            _parameter = parameter.InGameParameter;
            CurrentHp = MaxHp;
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
            BalanceChanged?.Invoke(Mathf.FloorToInt(CurrentHp));
        }

        public bool CanSpendHp(float amount)
        {
            return amount >= 0f && CurrentHp - amount >= _parameter.MinHpAfterSpend;
        }

        public bool TrySpendHp(float amount)
        {
            if (!CanSpendHp(amount))
            {
                return false;
            }

            SetCurrentHp(CurrentHp - amount);
            return true;
        }

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _uiManager = contextMap.GetRegisteredInstance<BloodLuckUIManager>();
            _gameStateManager = contextMap.GetRegisteredInstance<BloodLuckGameStateManager>();
            _gameResultManager = contextMap.GetRegisteredInstance<BloodLuckGameResultManager>();
        }

        public void TakeDamage(float amount)
        {
            float previousHp = CurrentHp;
            SetCurrentHp(CurrentHp - amount);
            if (CurrentHp < previousHp)
            {
                AudioManager.Instance?.PlayPlayerDamage();
            }
            if (CurrentHp <= 0)
            {
                HandlePlayerDeath();
            }
        }

        public void Heal(float amount)
        {
            SetCurrentHp(CurrentHp + amount);
        }

        public void HealFromDamageDealt(float damageDealt)
        {
            float healAmount = damageDealt * LifeStealRatio;
            if (healAmount <= 0f)
            {
                return;
            }

            Heal(healAmount);
        }

        int IShopWallet.Balance => Mathf.FloorToInt(CurrentHp);

        bool IShopWallet.CanSpend(int cost) => CanSpendHp(cost);

        bool IShopWallet.TrySpend(int cost) => TrySpendHp(cost);

        private void SetCurrentHp(float hp)
        {
            float clamped = Mathf.Clamp(hp, 0f, MaxHp);
            if (Mathf.Approximately(clamped, CurrentHp))
            {
                return;
            }

            CurrentHp = clamped;
            _uiManager.RefreshHud();
            BalanceChanged?.Invoke(Mathf.FloorToInt(CurrentHp));
        }

        private void HandlePlayerDeath()
        {
            if (_deathSoundPlayed) return;
            _deathSoundPlayed = true;
            AudioManager.Instance?.PlayDeath();
            _gameStateManager.FinishGame();
            _gameResultManager.ShowResult(BloodLuckGameResultType.EatenByVampires);
        }
    }
}
