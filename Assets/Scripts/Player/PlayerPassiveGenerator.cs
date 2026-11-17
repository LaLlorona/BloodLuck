namespace Player
{
    public static class PlayerPassiveGenerator
    {
        private enum Kind { Damage, MaxHp, AttackSpeed }
        
        private static (float min, float max, float step) GetRange(Kind kind, OperationMethod method)
        {
            return (kind, method) switch
            {
                (Kind.Damage,      OperationMethod.Add)      => (2f, 8f, 1f),
                (Kind.Damage,      OperationMethod.Multiply) => (1.05f, 1.2f, 0.05f),
                (Kind.MaxHp,       OperationMethod.Add)      => (10f, 40f, 5f),
                (Kind.MaxHp,       OperationMethod.Multiply) => (1.1f, 1.3f, 0.05f),
                (Kind.AttackSpeed, OperationMethod.Add)      => (0.1f, 0.3f, 0.05f),
                (Kind.AttackSpeed, OperationMethod.Multiply) => (1.1f, 1.3f, 0.05f),
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        private static readonly int KindCount = System.Enum.GetValues(typeof(Kind)).Length;
        
        public static System.Collections.Generic.List<PlayerPassive> GetRandomPlayerPassives(System.Random random, int count)
        {
            if (count > KindCount)
                throw new System.ArgumentOutOfRangeException(nameof(count), $"Only {KindCount} distinct kinds exist.");

            var kinds = new System.Collections.Generic.List<Kind>();
            for (int i = 0; i < KindCount; i++) kinds.Add((Kind)i);

            var result = new System.Collections.Generic.List<PlayerPassive>(count);
            for (int i = 0; i < count; i++)
            {
                int index = random.Next(kinds.Count);
                var kind = kinds[index];
                kinds.RemoveAt(index); // prevents duplicate kinds in one menu

                var method = random.Next(2) == 0 ? OperationMethod.Add : OperationMethod.Multiply;

                // ONLY ALLOW MULTIPLY OPERATION ON ATTACK SPEED
                if (kind == Kind.AttackSpeed)
                {
                    method = OperationMethod.Multiply;
                }

                var (min, max, step) = GetRange(kind, method);
                int steps = (int)System.Math.Round((max - min) / step);
                float value = min + random.Next(steps + 1) * step;
                value = (float)System.Math.Round(value, 2);

                result.Add(kind switch
                {
                    Kind.Damage      => new PlayerPassive.IncreasePlayerDamage(value, method),
                    Kind.MaxHp       => new PlayerPassive.IncreaseMaxHp(value, method),
                    Kind.AttackSpeed => new PlayerPassive.AttackSpeedUp(value, method),
                    _ => throw new System.ArgumentOutOfRangeException()
                });
            }
            return result;
        }
    }
}
