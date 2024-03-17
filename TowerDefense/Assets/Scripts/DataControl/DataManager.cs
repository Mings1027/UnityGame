using System;
using System.Collections;
using System.Collections.Generic;
using BackendControl;
using CustomEnumControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

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

    public readonly Dictionary<ItemType, ItemInfo> itemInfoTable = new();

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
    }
}