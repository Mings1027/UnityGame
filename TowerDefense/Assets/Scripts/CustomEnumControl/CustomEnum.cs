namespace CustomEnumControl
{
    public enum TowerType
    {
        Assassin,
        Ballista,
        Canon,
        Wizard,
        Defender,
        None
    }

    public enum SoundEnum
    {
        WaveStart,
        WaveEnd,
        ButtonSound,
        SellTower1,
        SellTower2,
        SellTower3
    }

    public enum UnitState
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }

    public enum PoolObjectKey
    {
        None,

        //==============Map==============
        ExpandButton,

        //==============Smoke==============
        ExpandMapSmoke,
        BuildSmoke,

        //==============Projectile==============
        BallistaProjectile,
        WizardProjectile,
        CanonProjectile,

        //==============Vfx==============

        BloodVfx,

        //==============Unit==============
        AssassinUnit,
        DefenderUnit,

        //==============Tower=============
        AssassinTower,
        BallistaTower,
        CanonTower,
        DefenceTower,
        WizardTower
    }

    public enum UIPoolObjectKey
    {
        UnitHealthBar,
        EnemyHealthBar,
        ReSpawnBar
    }

    public enum EnemyPoolObjectKey
    {
        Goblin,
        Orc,
        ArmoredGoblin,
        ArmoredOrc,
        Ooogie
    }
}