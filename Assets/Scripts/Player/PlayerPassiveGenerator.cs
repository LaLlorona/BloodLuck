namespace Player
{
    public static class PlayerPassiveGenerator
    {
        private enum Kind { Damage, MaxHp, AttackSpeed }
        
        private static (float min, float max, float step) GetRange(Kind kind, OperationMethod method)
        {
            return (kind, method) switch
            {
                (Kind.Damage,      OperationMethod.Add)      => (10f, 50f, 5f),
                (Kind.Damage,      OperationMethod.Multiply) => (1.1f, 1.5f, 0.05f),
                (Kind.MaxHp,       OperationMethod.Add)      => (10f, 40f, 5f),
                (Kind.MaxHp,       OperationMethod.Multiply) => (1.1f, 1.3f, 0.05f),
                (Kind.AttackSpeed, OperationMethod.Add)      => (0.1f, 0.3f, 0.05f),
                (Kind.AttackSpeed, OperationMethod.Multiply) => (1.1f, 1.3f, 0.05f),
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        public static PlayerPassive GetRandomPlayerPassive(System.Random random)
        {
            var kind = (Kind)random.Next(3);
            var method = random.Next(2) == 0 ? OperationMethod.Add : OperationMethod.Multiply;

            var (min, max, step) = GetRange(kind, method);
            int steps = (int)System.Math.Round((max - min) / step);
            float value = min + random.Next(steps + 1) * step;
            value = (float)System.Math.Round(value, 2);

            return kind switch
            {
                Kind.Damage      => new PlayerPassive.IncreasePlayerDamage(value, method),
                Kind.MaxHp       => new PlayerPassive.IncreaseMaxHp(value, method),
                Kind.AttackSpeed => new PlayerPassive.AttackSpeedUp(value, method),
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }
    }
}
