using BackEnd;
using UnityEngine;

namespace BackendControl
{
    public class BackendGameLog
    {
        private static BackendGameLog _instance;
        public static BackendGameLog instance => _instance ??= new BackendGameLog();

        public void GameLogInsert(string itemType, int itemQuantity, int itemPrice)
        {
            var param = new Param
            {
                { itemType, itemQuantity },
                { itemType, itemPrice }
            };

            Debug.Log("게임로그 삽입을 시도합니다.");

            var bro = Backend.GameLog.InsertLog("Item", param);

            if (bro.IsSuccess())
            {
                Debug.Log("게임로그 삽입에 성공했습니다. : " + bro);
            }
            else
            {
                Debug.LogError("게임로그 삽입 중 에러가 발생했습니다. : " + bro);
            }
        }
    }
}