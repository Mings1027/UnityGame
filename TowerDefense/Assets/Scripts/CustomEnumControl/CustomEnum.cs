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
        GameStart,
        WaveStart,
        WaveEnd,
        BossTheme,
        ButtonSound,
        LowCost,
        MediumCost,
        HighCost
    }

    public enum TowerState
    {
        Patrol,
        Attack
    }
    public enum UnitState
    {
        Patrol,
        Chase,
        Attack
    }

    public enum CamState
    {
        Move,
        CheckZoomRotate,
        Rotate,
        Zoom,
        Idle
    }

    public enum PoolObjectKey
    {
        None,

        //==============Map==============
        ExpandButton,

        //==============Smoke==============
        ExpandMapSmoke,
        BuildSmoke,
        ObstacleSmoke,

        //==============Projectile==============
        BallistaProjectile,
        WizardProjectile,
        CanonProjectile,

        //==============Vfx==============

        BloodVfx,

        //==============Tower=============
        AssassinUnit,
        DefenderUnit
    }

    public enum UIPoolObjectKey
    {
        UnitHealthBar,
        EnemyHealthBar,
        ReSpawnBar,
        FloatingText
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