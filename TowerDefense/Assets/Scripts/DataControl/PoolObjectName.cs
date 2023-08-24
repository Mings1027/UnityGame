namespace DataControl
{
    public static class PoolObjectName
    {
        public const string ExpandButton = "ExpandButton";

        /*==================================================================================================================
        *                                              Tower
        ==================================================================================================================*/
        public const string ExpandMapSmoke = "ExpandMapSmoke";
        public const string BuildSmoke = "BuildSmoke";
        public const string UnitSpawnSmoke = "UnitSpawnSmoke";

        /*==================================================================================================================
        *                                              Bullet
        ==================================================================================================================*/
        public const string BallistaProjectile = "BallistaProjectile";
        public const string MageBullet = "MageBullet";
        public const string CanonBullet = "CanonBullet";

        public const string EnemyArrow = "EnemyArrow";

        /*==================================================================================================================
        *                                              Sound
        ==================================================================================================================*/
        /*------------------------------------------------------------------------------------------------------------------
        *                                               Attack Sound
        ------------------------------------------------------------------------------------------------------------------*/

        public const string BallistaShootSfx = "ArrowShootSFX";
        public const string CanonShootSfx = "CanonShootSFX";
        public const string MageShootSfx = "MageShootSFX";

        /*------------------------------------------------------------------------------------------------------------------
        *                                               Hit Sound
        ------------------------------------------------------------------------------------------------------------------*/
        public const string BallistaHitSfx = "ArrowHitSFX";
        public const string CanonHitSfx = "CanonHitSFX";

        /*==================================================================================================================
        *                                              Effect
        ==================================================================================================================*/
        public static string[] ballistaVfx = { BallistaVfx1, BallistaVfx2, BallistaVfx3 };

        private const string BallistaVfx1 = "BallistaVfx1";
        private const string BallistaVfx2 = "BallistaVfx2";
        private const string BallistaVfx3 = "BallistaVfx3";
    }
}