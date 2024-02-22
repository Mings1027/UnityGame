namespace CustomEnumControl
{
    public enum LoginPlatform
    {
        Apple = 1,
        Google = 2,
        Custom = 3
    }

    public enum Difficulty
    {
        Single = 0,
        Double = 1,
        Triple = 2,
        Quadruple = 3
    }

    public enum TowerType
    {
        Assassin,
        Ballista,
        Canon,
        Wizard,
        Defender,
        Laser,
        ManaCrystal,
        None
    }

    public enum ItemType
    {
        None,
        NuclearBomb,
        MoneyBag,
        TowerHeart
    }

    public enum PriceType
    {
        Diamond,
        Emerald
    }

    public enum Currency
    {
        Korea,
        Usa,
        Japan
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
        Detect,

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
        TransformSmoke,
        BallistaHitParticle,
        CanonHitParticle,
        WizardHitParticle,
        NuclearBomb,
        
        BallistaTrailParticle,
        CanonTrailParticle,
        WizardTrailParticle
    }

    public enum UIPoolObjectKey
    {
        UnitHealthBar,
        MonsterHealthBar,
        BossHealthBar,
        ReSpawnBar,
        FloatingText,
        HealText,
        DamageText,
        MoneyText,
        TowerHealText,
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
        
        BossOoogie,
        BossReaperOoogie,
        BossSphinx
    }
}