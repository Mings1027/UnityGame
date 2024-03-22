using System;
using System.Collections.Generic;
using BackendControl;
using CustomEnumControl;
using ManagerControl;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace DataControl
{
    public abstract class TowerDataManager
    {
        public class ItemInfo
        {
            public string itemName;
            public int itemCount;
            public string itemDescription;

            public ItemInfo(string itemName, int itemCount, string itemDescription)
            {
                this.itemName = itemName;
                this.itemCount = itemCount;
                this.itemDescription = itemDescription;
            }
        }

        public class TowerInfo
        {
            public string towerName;
            public string towerDescription;

            public TowerInfo(string towerName, string towerDescription)
            {
                this.towerName = towerName;
                this.towerDescription = towerDescription;
            }
        }

        public static readonly Dictionary<ItemType, ItemInfo> ItemInfoTable = new();
        public static readonly Dictionary<TowerType, TowerInfo> TowerInfoTable = new();

        public static void Init()
        {
            var itemInventory = BackendGameData.userData.itemInventory;
            if (ItemInfoTable.Count == 0)
            {
                var itemTypeArray = Enum.GetValues(typeof(ItemType));
                foreach (ItemType itemType in itemTypeArray)
                {
                    ItemInfoTable.Add(itemType,
                        new ItemInfo(
                            LocaleManager.GetLocalizedString(LocaleManager.ItemTable, itemType.ToString()),
                            itemInventory[itemType.ToString()],
                            LocaleManager.GetLocalizedString(LocaleManager.ItemDescriptionTable, itemType.ToString())));
                }
            }

            if (TowerInfoTable.Count == 0)
            {
                var towerTypes = Enum.GetValues(typeof(TowerType));
                foreach (TowerType towerType in towerTypes)
                {
                    if (towerType == TowerType.None) continue;
                    TowerInfoTable.Add(towerType, new TowerInfo(
                        LocaleManager.GetLocalizedString(LocaleManager.TowerCardTable, towerType.ToString()),
                        LocaleManager.GetLocalizedString(LocaleManager.TowerDescriptionTable, towerType.ToString())));
                }
            }

            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleItemTable;
        }

        public static void RemoveLocaleMethod()
        {
            LocalizationSettings.SelectedLocaleChanged -= ChangeLocaleItemTable;
        }

        private static void ChangeLocaleItemTable(Locale locale)
        {
            var itemInventory = BackendGameData.userData.itemInventory;
            var itemTypeArray = Enum.GetValues(typeof(ItemType));
            foreach (ItemType itemType in itemTypeArray)
            {
                ItemInfoTable[itemType].itemName =
                    LocaleManager.GetLocalizedString(LocaleManager.ItemTable, itemType.ToString());
                ItemInfoTable[itemType].itemCount =
                    itemInventory[itemType.ToString()];
                ItemInfoTable[itemType].itemDescription =
                    LocaleManager.GetLocalizedString(LocaleManager.ItemDescriptionTable, itemType.ToString());
            }

            var towerTypes = Enum.GetValues(typeof(TowerType));
            foreach (TowerType towerType in towerTypes)
            {
                if (towerType == TowerType.None) continue;
                TowerInfoTable[towerType].towerName =
                    LocaleManager.GetLocalizedString(LocaleManager.TowerCardTable, towerType.ToString());
                TowerInfoTable[towerType].towerDescription =
                    LocaleManager.GetLocalizedString(LocaleManager.TowerDescriptionTable, towerType.ToString());
            }
        }
    }
}