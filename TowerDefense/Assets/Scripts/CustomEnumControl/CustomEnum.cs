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