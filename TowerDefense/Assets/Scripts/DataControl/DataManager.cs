using System;
using System.Collections.Generic;
using BackendControl;
using CustomEnumControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace DataControl
{
    public class DataManager : MonoBehaviour
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

        public readonly Dictionary<ItemType, ItemInfo> itemInfoTable = new();
        public readonly Dictionary<TowerType, TowerInfo> towerInfoTable = new();

        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleItemTable;
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= ChangeLocaleItemTable;
        }

        private void Init()
        {
            var itemInventory = BackendGameData.userData.itemInventory;
            var itemTypeArray = Enum.GetValues(typeof(ItemType));
            foreach (ItemType itemType in itemTypeArray)
            {
                itemInfoTable.Add(itemType,
                    new ItemInfo(
                        LocaleManager.GetLocalizedString(LocaleManager.ItemTable, itemType.ToString()),
                        itemInventory[itemType.ToString()],
                        LocaleManager.GetLocalizedString(LocaleManager.ItemDescriptionTable, itemType.ToString())));
            }

            var towerTypes = Enum.GetValues(typeof(TowerType));
            foreach (TowerType towerType in towerTypes)
            {
                if (towerType == TowerType.None) continue;
                towerInfoTable.Add(towerType, new TowerInfo(
                    LocaleManager.GetLocalizedString(LocaleManager.TowerCardTable, towerType.ToString()),
                    LocaleManager.GetLocalizedString(LocaleManager.TowerDescriptionTable, towerType.ToString())));
            }
        }

        private void ChangeLocaleItemTable(Locale locale)
        {
            var itemInventory = BackendGameData.userData.itemInventory;
            var itemTypeArray = Enum.GetValues(typeof(ItemType));
            foreach (ItemType itemType in itemTypeArray)
            {
                itemInfoTable[itemType].itemName =
                    LocaleManager.GetLocalizedString(LocaleManager.ItemTable, itemType.ToString());
                itemInfoTable[itemType].itemCount =
                    itemInventory[itemType.ToString()];
                itemInfoTable[itemType].itemDescription =
                    LocaleManager.GetLocalizedString(LocaleManager.ItemDescriptionTable, itemType.ToString());
            }

            var towerTypes = Enum.GetValues(typeof(TowerType));
            foreach (TowerType towerType in towerTypes)
            {
                if (towerType == TowerType.None) continue;
                towerInfoTable[towerType].towerName =
                    LocaleManager.GetLocalizedString(LocaleManager.TowerCardTable, towerType.ToString());
                towerInfoTable[towerType].towerDescription =
                    LocaleManager.GetLocalizedString(LocaleManager.TowerDescriptionTable, towerType.ToString());
            }
        }
    }
}