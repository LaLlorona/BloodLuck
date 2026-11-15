using System.Collections.Generic;

namespace Player
{
    public class PlayerPassiveContainer
    {
        private readonly List<PlayerPassive> _passives = new();

        public IReadOnlyList<PlayerPassive> Passives => _passives;

        public void AddPassive(PlayerPassive passive)
        {
            _passives.Add(passive);
        }

        public bool RemovePassive(PlayerPassive passive)
        {
            return _passives.Remove(passive);
        }

        public void Clear()
        {
            _passives.Clear();
        }

        public float GetMaxHp(float baseMaxHp)
        {
            float add = 0f;
            float multiply = 1f;
            foreach (PlayerPassive passive in _passives)
            {
                if (passive is PlayerPassive.IncreaseMaxHp p)
                {
                    Accumulate(p.PassiveValue, p.OperationMethod, ref add, ref multiply);
                }
            }

            return (baseMaxHp + add) * multiply;
        }

        public float GetAttackSpeed(float baseAttackSpeed)
        {
            float add = 0f;
            float multiply = 1f;
            foreach (PlayerPassive passive in _passives)
            {
                if (passive is PlayerPassive.AttackSpeedUp p)
                {
                    Accumulate(p.PassiveValue, p.OperationMethod, ref add, ref multiply);
                }
            }

            return (baseAttackSpeed + add) * multiply;
        }

        public float GetDamage(float baseDamage)
        {
            float add = 0f;
            float multiply = 1f;
            foreach (PlayerPassive passive in _passives)
            {
                if (passive is PlayerPassive.IncreasePlayerDamage p)
                {
                    Accumulate(p.PassiveValue, p.OperationMethod, ref add, ref multiply);
                }
            }

            return (baseDamage + add) * multiply;
        }

        private static void Accumulate(float value, OperationMethod method, ref float add, ref float multiply)
        {
            switch (method)
            {
                case OperationMethod.Add:
                    add += value;
                    break;
                case OperationMethod.Multiply:
                    multiply *= value;
                    break;
            }
        }
    }
}
