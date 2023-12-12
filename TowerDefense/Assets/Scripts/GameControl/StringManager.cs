using System.Collections.Generic;
using CustomEnumControl;

namespace GameControl
{
    public static class StringManager
    {
        public const string Xp = "XP";
        public const string UpgradeCount = "UpgradeCount";
        // public const string Mana = "Mana";

        public static readonly Dictionary<TowerType, string> DamageDic = new()
        {
            { TowerType.Assassin, "AssassinDamage" },
            { TowerType.Ballista, "BallistaDamage" },
            { TowerType.Canon, "CanonDamage" },
            { TowerType.Defender, "DefenderDamage" },
            { TowerType.Laser, "LaserDamage" },
            { TowerType.Wizard, "WizardDamage" }
        };

        public static readonly Dictionary<TowerType, string> RangeDic = new()
        {
            { TowerType.Assassin, "AssassinRange" },
            { TowerType.Ballista, "BallistaRange" },
            { TowerType.Canon, "CanonRange" },
            { TowerType.Defender, "DefenderRange" },
            { TowerType.Laser, "LaserRange" },
            { TowerType.Wizard, "WizardRange" }
        };
    }
}