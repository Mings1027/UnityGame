using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// 뒤끝 SDK namespace 추가
using BackEnd;
using CustomEnumControl;
using GameControl;

public class UserData
{
    public int diamonds;

    public Dictionary<ItemType, int> itemInventory = new();

    // 데이터를 디버깅하기 위한 함수입니다.(Debug.Log(UserData);)
    public override string ToString()
    {
        var result = new StringBuilder();
        result.AppendLine($"diamonds : {diamonds}");
        foreach (var itemKey in itemInventory.Keys)
        {
            result.AppendLine($"$ {itemKey} : {itemInventory[itemKey]} 개");
        }

        return result.ToString();
    }
}

public class BackendGameData
{
    private static BackendGameData _instance;
    public static BackendGameData instance => _instance ??= new BackendGameData();

    public static UserData userData { get; private set; }

    private string _gameDataRowInDate = string.Empty;

    public void GameDataInsert()
    {
        userData ??= new UserData();
        Debug.Log("데이터를 초기화 합니다.");
        userData.diamonds = 0;
        var itemTypes = Enum.GetValues(typeof(ItemType));
        foreach (ItemType itemType in itemTypes)
        {
            if (itemType == ItemType.None) continue;
            userData.itemInventory.Add(itemType, 0);
        }

        var param = new Param
        {
            { "diamonds", userData.diamonds },
            { "inventory", userData.itemInventory }
        };
        Debug.Log("게임정보 데이터 삽입을 요청합니다.");
        var bro = Backend.GameData.Insert("USER_DATA", param);
        if (bro.IsSuccess())
        {
            Debug.Log("게임정보 데이터 삽입에 성공했습니다. :" + bro);
        }
        else
        {
            Debug.LogError("게임정보 데이터 삽입에 실패했습니다. : " + bro);
        }
    }

    public void GameDataGet()
    {
        // Step 3. 게임정보 불러오기 구현하기
    }

    public void LevelUp()
    {
        // Step 4. 게임정보 수정 구현하기
    }

    public void GameDataUpdate()
    {
        // Step 4. 게임정보 수정 구현하기
    }
}