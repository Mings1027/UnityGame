namespace CustomEnumControl
{
    public enum TowerType
    {
        Assassin,
        Ballista,
        Canon,
        Wizard,
        Defender,
        Laser,
        Mana,
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
        DefenderUnit,
        TransformSmoke
    }

    public enum UIPoolObjectKey
    {
        UnitHealthBar,
        EnemyHealthBar,
        BossHealthBar,
        ReSpawnBar,
        FloatingText,
        HealText,
        DamageText
    }

    public enum MonsterPoolObjectKey
    {
        Goblin,
        Orc,
        ArmoredGoblin,
        ArmoredOrc,
        Troll,
        SiegeRam,

        Zombie,
        Skeleton,
        Spider,
        Mummy,
        Lich,
        
        Witch,
        Bat,
        Vampire,
        JackOLantern,
        Werewolf,
        
        Santa,
        Reindeer,
        SnowMan,
    }
}