namespace DataControl
{
    public abstract class PoolObjectName
    {
        /*==================================================================================================================
        *                                              Tower  
        ==================================================================================================================*/
        public const string BuildingPoint = "BuildingPoint";
        public const string BuildSmoke = "BuildSmoke";

        /*==================================================================================================================
        *                                              Unit  
        ==================================================================================================================*/
        public const string ArcherUnit = "ArcherUnit";
        public const string SwordManUnit = "SwordManUnit";
        public const string SpearManUnit = "SpearManUnit";

        /*==================================================================================================================
        *                                              Bullet  
        ==================================================================================================================*/
        public const string ArcherBullet = "ArcherBullet";
        public const string ArcherProjectile = "ArcherProjectile";
        public const string MageBullet = "MageBullet";
        public const string SlowDownMageBullet = "SlowDownMageBullet";
        public const string PenetrationMageBullet = "PenetrationMageBullet";
        public const string CanonBullet = "CanonBullet";

        public const string EnemyArrow = "EnemyArrow";

        /*==================================================================================================================
        *                                              Sound  
        ==================================================================================================================*/
        /*------------------------------------------------------------------------------------------------------------------
        *                                               Attack Sound
        ------------------------------------------------------------------------------------------------------------------*/
        
        public const string ArrowShootSfx = "ArrowShootSFX";
        public const string BulletShootSfx = "BulletShootSFX";
        
        public const string SwordSlashSfx = "SwordSlashSFX";
        
        public const string CanonShootSfx = "CanonShootSFX";
        
        public const string MageShootSfx = "MageShootSFX";
        public const string SlowDownMageShootSfx = "SlowDownMageShootSfx";
        public const string PenetrationMageShootSfx = "PenetrationMageShootSfx";
        /*------------------------------------------------------------------------------------------------------------------
        *                                               Hit Sound
        ------------------------------------------------------------------------------------------------------------------*/
        public const string ArrowHitSfx = "ArrowHitSFX";
        public const string BulletHitSfx = "BulletHitSFX";
        public const string CanonHitSfx = "CanonHitSFX";

        /*==================================================================================================================
        *                                              Effect  
        ==================================================================================================================*/
        /*------------------------------------------------------------------------------------------------------------------
        *                                            Attack Effect  
        ------------------------------------------------------------------------------------------------------------------*/
        public const string SlashVFX = "SlashVFX";
        
        /*------------------------------------------------------------------------------------------------------------------
        *                                            Hit Effect  
        ------------------------------------------------------------------------------------------------------------------*/
        public const string ArrowHitVFX = "ArrowHitVFX";
        public const string BulletHitVFX = "BulletHitVFX";
        public const string SlashHitVFX = "SlashHitVFX";
        public const string CanonHitVFX = "CanonHitVFX";
        public const string MageHitVFX = "MageHitVFX";

        /*==================================================================================================================
        *                                            Tower Effect  
        ==================================================================================================================*/
        public const string CanonSmoke = "CanonSmoke";
    }
}