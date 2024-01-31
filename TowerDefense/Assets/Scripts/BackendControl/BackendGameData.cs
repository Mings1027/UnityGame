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
        public int emerald;
        public int xp;
        public int totalSpentXp;
        public int score;

        public readonly List<string> survivedWaveList = new(4);
        public readonly Dictionary<string, int> itemInventory = new();
        public readonly Dictionary<string, int> towerLevelTable = new();

        // 데이터를 디버깅하기 위한 함수입니다.
        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine($"에메랄드 : {emerald}");
            result.AppendLine($"xp : {xp}");
            result.AppendLine($"totalSpentXp : {totalSpentXp}");
            result.AppendLine($"점수 : {score}");
            for (var i = 0; i < survivedWaveList.Count; i++)
            {
                result.AppendLine($"난이도 : {i} , 생존웨이브 : {survivedWaveList[i]}");
            }

            foreach (var itemKey in itemInventory.Keys)
            {
                result.AppendLine($"{itemKey} : {itemInventory[itemKey]} 개");
            }

            foreach (var key in towerLevelTable.Keys)
            {
                result.AppendLine($"{key}레벨 : {towerLevelTable[key]}");
            }

            return result.ToString();
        }
    }

    public class BackendGameData
    {
        private static BackendGameData _instance;
        public static BackendGameData instance => _instance ??= new BackendGameData();

        private const string UserDataTable = "USER_DATA";
        private string _gameDataRowInDate = string.Empty;
        private byte _difficultyLevel;
        private byte _lastSurvivedWave;

        public static UserData userData { get; private set; }
        public static byte scoreMultiplier { get; set; }
        public static int curTbc { get; set; }

        public void SetLevel(byte difficulty)
        {
            _difficultyLevel = difficulty;
            _lastSurvivedWave = byte.Parse(userData.survivedWaveList[_difficultyLevel]);
            CustomLog.Log($"선택한 난이도 : {_difficultyLevel}");
            CustomLog.Log($"마지막 생존 웨이브 : {_lastSurvivedWave}");
        }

        public void UpdateSurvivedWave(byte wave)
        {
            if (_lastSurvivedWave <= byte.Parse(userData.survivedWaveList[_difficultyLevel]))
            {
                userData.survivedWaveList[_difficultyLevel] = wave.ToString();
                CustomLog.Log($"생존웨이브 업데이트 : {userData.survivedWaveList[_difficultyLevel]}");
                CalculateTotalScore(wave);
                CalculateXp();
            }
        }

        private void CalculateTotalScore(byte wave)
        {
            var totalScore = wave * scoreMultiplier;
            var userScore = userData.score;
            if (userScore < totalScore)
            {
                userData.score = totalScore;
                BackendRank.instance.RankInsert(userData.score);
            }
        }

        private void CalculateXp()
        {
            var prevXp = userData.xp;
            var earnedXp = byte.Parse(userData.survivedWaveList[_difficultyLevel]) *
                byte.Parse(userData.survivedWaveList[_difficultyLevel] + 1) * _difficultyLevel / 2;
            userData.xp = prevXp + earnedXp;
        }

        public void GameDataInsert()
        {
            userData ??= new UserData();
            CustomLog.Log("데이터를 초기화 합니다.");
            userData.emerald = 0;
            userData.xp = 0;
            userData.totalSpentXp = 0;
            userData.score = 0;

            for (var i = 0; i < 4; i++)
            {
                userData.survivedWaveList.Add("0");
                CustomLog.Log($"난이도 : {i}");
            }

            var itemTypes = Enum.GetValues(typeof(ItemType));
            foreach (ItemType itemType in itemTypes)
            {
                if (itemType == ItemType.None) continue;
                userData.itemInventory.Add(itemType.ToString(), 0);
            }

            var towerTypes = Enum.GetValues(typeof(TowerType));
            foreach (TowerType towerType in towerTypes)
            {
                if (towerType == TowerType.None) continue;
                userData.towerLevelTable.Add(towerType.ToString(), 0);
            }

            var param = new Param
            {
                { "emerald", userData.emerald },
                { "xp", userData.xp },
                { "totalSpentXp", userData.totalSpentXp },
                { "score", userData.score },
                { "survivedWaveList", userData.survivedWaveList },
                { "itemInventory", userData.itemInventory },
                { "towerLevelTable", userData.towerLevelTable },
            };
            CustomLog.Log("게임정보 데이터 삽입을 요청합니다.");
            var bro = Backend.GameData.Insert(UserDataTable, param);
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
            var bro = Backend.GameData.GetMyData(UserDataTable, new Where());
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
                    CustomLog.Log("데이터가 존재합니다.");
                    _gameDataRowInDate = gameDataJson[0]["inDate"].ToString(); //불러온 게임정보의 고유값입니다.  

                    userData = new UserData
                    {
                        emerald = int.Parse(gameDataJson[0]["emerald"].ToString()),
                        xp = int.Parse(gameDataJson[0]["xp"].ToString()),
                        score = int.Parse(gameDataJson[0]["score"].ToString())
                    };
                    CustomLog.Log("웨이브 가져오는중");

                    foreach (var waveKey in gameDataJson[0]["survivedWaveList"])
                    {
                        userData.survivedWaveList.Add(waveKey.ToString());
                    }

                    CustomLog.Log($"난이도개수 : {userData.survivedWaveList.Count}");

                    foreach (var itemKey in gameDataJson[0]["itemInventory"].Keys)
                    {
                        userData.itemInventory.Add(itemKey,
                            int.Parse(gameDataJson[0]["itemInventory"][itemKey].ToString()));
                    }

                    foreach (var towerKey in gameDataJson[0]["towerLevelTable"].Keys)
                    {
                        userData.towerLevelTable.Add(towerKey,
                            int.Parse(gameDataJson[0]["towerLevelTable"][towerKey].ToString()));
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
                { "emerald", userData.emerald },
                { "xp", userData.xp },
                { "totalSpentXp", userData.totalSpentXp },
                { "score", userData.score },
                { "survivedWaveList", userData.survivedWaveList },
                { "itemInventory", userData.itemInventory },
                { "towerLevelTable", userData.towerLevelTable },
            };

            BackendReturnObject bro;

            if (string.IsNullOrEmpty(_gameDataRowInDate))
            {
                CustomLog.Log("내 제일 최신 게임정보 데이터 수정을 요청합니다.");

                bro = Backend.GameData.Update(UserDataTable, new Where(), param);
            }
            else
            {
                CustomLog.Log($"{_gameDataRowInDate}의 게임정보 데이터 수정을 요청합니다.");

                bro = Backend.GameData.UpdateV2(UserDataTable, _gameDataRowInDate, Backend.UserInDate, param);
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