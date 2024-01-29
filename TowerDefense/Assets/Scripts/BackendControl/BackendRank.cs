using System.Text;
using BackEnd;
using UnityEngine;

namespace BackendControl
{
    public struct UserRankInfo
    {
        public string rank;
        public string nickName;
        public string score;
    }

    public class BackendRank
    {
        private static BackendRank _instance;
        public static BackendRank instance => _instance ??= new BackendRank();

        public void RankInsert(int score)
        {
            // [변경 필요] '복사한 UUID 값'을 '뒤끝 콘솔 > 랭킹 관리'에서 생성한 랭킹의 UUID값으로 변경해주세요.  
            const string
                rankUuid = "d1f25cc0-bbf4-11ee-8df0-118b3eecd33b"; // 예시 : "4088f640-693e-11ed-ad29-ad8f0c3d4c70"
            const string tableName = "USER_DATA";
            string rowInDate;

            // 랭킹을 삽입하기 위해서는 게임 데이터에서 사용하는 데이터의 inDate값이 필요합니다.  
            // 따라서 데이터를 불러온 후, 해당 데이터의 inDate값을 추출하는 작업을 해야합니다.  
            Debug.Log("데이터 조회를 시도합니다.");
            var bro = Backend.GameData.GetMyData(tableName, new Where());

            if (bro.IsSuccess() == false)
            {
                Debug.LogError("데이터 조회 중 문제가 발생했습니다 : " + bro);
                return;
            }

            Debug.Log("데이터 조회에 성공했습니다 : " + bro);

            if (bro.FlattenRows().Count > 0)
            {
                rowInDate = bro.FlattenRows()[0]["inDate"].ToString();
            }
            else
            {
                Debug.Log("데이터가 존재하지 않습니다. 데이터 삽입을 시도합니다.");
                var bro2 = Backend.GameData.Insert(tableName);

                if (bro2.IsSuccess() == false)
                {
                    Debug.LogError("데이터 삽입 중 문제가 발생했습니다 : " + bro2);
                    return;
                }

                Debug.Log("데이터 삽입에 성공했습니다 : " + bro2);

                rowInDate = bro2.GetInDate();
            }

            Debug.Log("내 게임 정보의 rowInDate : " + rowInDate); // 추출된 rowIndate의 값은 다음과 같습니다.  

            var param = new Param();
            param.Add("score", score);

            // 추출된 rowIndate를 가진 데이터에 param값으로 수정을 진행하고 랭킹에 데이터를 업데이트합니다.  
            Debug.Log("랭킹 삽입을 시도합니다.");
            var rankBro = Backend.URank.User.UpdateUserScore(rankUuid, tableName, rowInDate, param);

            if (rankBro.IsSuccess() == false)
            {
                Debug.LogError("랭킹 등록 중 오류가 발생했습니다. : " + rankBro);
                return;
            }

            Debug.Log("랭킹 삽입에 성공했습니다. : " + rankBro);
        }

        public UserRankInfo[] RankGet()
        {
            const string rankUuid = "d1f25cc0-bbf4-11ee-8df0-118b3eecd33b";
            var bro = Backend.URank.User.GetRankList(rankUuid);

            if (!bro.IsSuccess())
            {
                Debug.LogError("랭킹 조회중 오류가 발생했습니다. : " + bro);
                return null;
            }

            Debug.Log("랭킹 조회에 성공했습니다. : " + bro);

            Debug.Log("총 랭킹 등록 유저 수 : " + bro.GetFlattenJSON()["totalCount"]);

            var userRankInfo = new UserRankInfo[bro.FlattenRows().Count];
            var index = 0;
            foreach (LitJson.JsonData jsonData in bro.FlattenRows())
            {
                userRankInfo[index].rank = jsonData["rank"].ToString();
                userRankInfo[index].nickName = jsonData["nickname"].ToString();
                userRankInfo[index].score = jsonData["score"].ToString();
                index += 1;
                // var info = new StringBuilder();
                // info.AppendLine("순위 : " + jsonData["rank"]);
                // info.AppendLine("닉네임 : " + jsonData["nickname"]);
                // info.AppendLine("점수 : " + jsonData["score"]);
                // info.AppendLine("gamerInDate : " + jsonData["gamerInDate"]);
                // info.AppendLine("정렬번호 : " + jsonData["index"]);
                // info.AppendLine();
                // Debug.Log(info);
            }

            return userRankInfo;
        }
    }
}