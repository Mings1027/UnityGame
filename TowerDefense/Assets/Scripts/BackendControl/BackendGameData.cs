using System;
using System.Collections.Generic;
using System.Text;
using BackEnd;
using CustomEnumControl;
using UnityEngine;
using Utilities;

namespace BackendControl
{
    public class UserData
    {
        public int diamonds;
        public int score;
        public readonly Dictionary<string, int> itemInventory = new();

        // 데이터를 디버깅하기 위한 함수입니다.
        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine($"diamonds : {diamonds}");
            result.AppendLine($"score : {score}");

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
            CustomLog.Log("데이터를 초기화 합니다.");
            userData.diamonds = 0;

            userData.score = 0;

            var itemTypes = Enum.GetValues(typeof(ItemType));
            foreach (ItemType itemType in itemTypes)
            {
                if (itemType == ItemType.None) continue;
                userData.itemInventory.Add(itemType.ToString(), 0);
            }

            var param = new Param
            {
                { "diamonds", userData.diamonds },
                { "score", userData.score },
                { "itemInventory", userData.itemInventory }
            };
            CustomLog.Log("게임정보 데이터 삽입을 요청합니다.");
            var bro = Backend.GameData.Insert("USER_DATA", param);
            if (bro.IsSuccess())
            {
                CustomLog.Log("게임정보 데이터 삽입에 성공했습니다. :" + bro);
                _gameDataRowInDate = bro.GetInDate();
            }
            else
            {
                CustomLog.LogError("게임정보 데이터 삽입에 실패했습니다. : " + bro);
            }
        }

        public void GameDataGet()
        {
            CustomLog.Log("게임 정보 조회 함수를 호출합니다.");
            var bro = Backend.GameData.GetMyData("USER_DATA", new Where());
            if (bro.IsSuccess())
            {
                CustomLog.Log("게임 정보 조회에 성공했습니다. : " + bro);

                var gameDataJson = bro.FlattenRows(); // Json으로 리턴된 데이터를 받아옵니다.  

                // 받아온 데이터의 갯수가 0이라면 데이터가 존재하지 않는 것입니다.  
                if (gameDataJson.Count <= 0)
                {
                    CustomLog.LogWarning("데이터가 존재하지 않습니다.");
                }
                else
                {
                    _gameDataRowInDate = gameDataJson[0]["inDate"].ToString(); //불러온 게임정보의 고유값입니다.  

                    userData = new UserData
                    {
                        diamonds = int.Parse(gameDataJson[0]["diamonds"].ToString())
                    };

                    foreach (var itemKey in gameDataJson[0]["itemInventory"].Keys)
                    {
                        userData.itemInventory.Add(itemKey,
                            int.Parse(gameDataJson[0]["itemInventory"][itemKey].ToString()));
                    }

                    CustomLog.Log(userData.ToString());
                }
            }
            else
            {
                CustomLog.LogError("게임 정보 조회에 실패했습니다. : " + bro);
            }
        }

        public void LevelUp()
        {
            // Step 4. 게임정보 수정 구현하기
        }

        public void GameDataUpdate()
        {
            if (userData == null)
            {
                CustomLog.LogError("서버에서 다운받거나 새로 삽입한 데이터가 존재하지 않습니다. Insert 혹은 Get을 통해 데이터를 생성해주세요.");
                return;
            }

            var param = new Param
            {
                { "diamonds", userData.diamonds },
                { "itemInventory", userData.itemInventory }
            };

            BackendReturnObject bro;

            if (string.IsNullOrEmpty(_gameDataRowInDate))
            {
                CustomLog.Log("내 제일 최신 게임정보 데이터 수정을 요청합니다.");

                bro = Backend.GameData.Update("USER_DATA", new Where(), param);
            }
            else
            {
                CustomLog.Log($"{_gameDataRowInDate}의 게임정보 데이터 수정을 요청합니다.");

                bro = Backend.GameData.UpdateV2("USER_DATA", _gameDataRowInDate, Backend.UserInDate, param);
            }

            if (bro.IsSuccess())
            {
                CustomLog.Log("게임정보 데이터 수정에 성공했습니다. : " + bro);
            }
            else
            {
                CustomLog.LogError("게임정보 데이터 수정에 실패했습니다. : " + bro);
            }
        }
    }
}