namespace InterfaceControl
{
    public interface IUnit
    {
        int UnitHealth { get; set; }
        int UnitDamage { get; set; }
        float AttackDelay { get; set; }
        float AttackRange { get; set; }

        void InitHealth(int amount);

        void InitDamage(int amount);

        void InitAttackDelay(float amount);

        void InitAttackRange(float amount);
    }
}