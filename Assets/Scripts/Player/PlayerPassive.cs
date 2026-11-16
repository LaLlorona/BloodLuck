namespace Player
{
    public enum OperationMethod
    {
        Multiply,
        Add
    }
    public abstract record PlayerPassive
    {
        public abstract string GetTitle();

        public abstract string GetDescription();

        public abstract string IconName { get; }

        private static string FormatOperation(string label, float value, OperationMethod method)
        {
            return method == OperationMethod.Add
                ? $"- {label} + {value:0.##}"
                : $"- {label} * {value:0.##}";
        }

        public sealed record IncreaseMaxHp(float PassiveValue, OperationMethod OperationMethod) : PlayerPassive
        {
            public override string IconName => "hp";

            public override string GetTitle() => "Your Max hp increases";

            public override string GetDescription() => FormatOperation("Max HP", PassiveValue, OperationMethod);
        }

        public sealed record IncreasePlayerDamage(float PassiveValue, OperationMethod OperationMethod) : PlayerPassive
        {
            public override string IconName => "sword";

            public override string GetTitle() => "Your Attack deals more damage";

            public override string GetDescription() => FormatOperation("Damage", PassiveValue, OperationMethod);
        }

        public sealed record AttackSpeedUp(float PassiveValue, OperationMethod OperationMethod) : PlayerPassive
        {
            public override string IconName => "bracers";

            public override string GetTitle() => "You attack speed increases";

            public override string GetDescription() => FormatOperation("Attack Speed", PassiveValue, OperationMethod);
        }

        public sealed record DoubleDash() : PlayerPassive
        {
            public override string IconName => "boots";

            public override string GetTitle() => "You can dash twice";

            public override string GetDescription() => "- Dash charges + 1";
        }

        public sealed record DashDamage(float PassiveValue, OperationMethod OperationMethod) : PlayerPassive
        {
            public override string IconName => "gloves";

            public override string GetTitle() => "Your dash hurts monsters";

            public override string GetDescription() => FormatOperation("Dash Damage", PassiveValue, OperationMethod);
        }

        public sealed record ChainLightning(float PassiveValue) : PlayerPassive
        {
            public override string IconName => "scroll";

            public override string GetTitle() => "Lightning chains between monsters";

            public override string GetDescription() => $"- Chain lightning damage + {PassiveValue:0.##}";
        }

        public sealed record ThirdAttackBonus(float PassiveValue, OperationMethod OperationMethod) : PlayerPassive
        {
            public override string IconName => "axe";

            public override string GetTitle() => "Your third attack hits harder";

            public override string GetDescription() => FormatOperation("Third Attack Damage", PassiveValue, OperationMethod);
        }

        public sealed record LifeSteal(float PassiveValue) : PlayerPassive
        {
            public override string IconName => "Meat";

            public override string GetTitle() => "You steal life from monsters";

            public override string GetDescription() => $"- Heal {PassiveValue * 100f:0.##}% of damage dealt";
        }

        public sealed record SPECIAL_ABILITIES_HAHA() : PlayerPassive
        {
            public override string IconName => "gem";

            public override string GetTitle() => "Something special happens";

            public override string GetDescription() => "- ???";
        }
    }
}
