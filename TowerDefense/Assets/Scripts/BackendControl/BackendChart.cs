using System.Collections.Generic;
using BackEnd;
using UnityEngine;

namespace BackendControl
{
    public class BackendChart
    {
        private static BackendChart _instance;
        public static BackendChart instance => _instance ??= new BackendChart();
        public static readonly Dictionary<string, int> ItemTable = new();

        public void ChartGet()
        {
            const string chartId = "106985";
            Debug.Log($"{chartId}의 차트 불러오기를 요청합니다.");
            var bro = Backend.Chart.GetChartContents(chartId);

            if (bro.IsSuccess() == false)
            {
                Debug.LogError($"{chartId}의 차트를 불러오는 중, 에러가 발생했습니다. : " + bro);
                return;
            }

            Debug.Log("차트 불러오기에 성공했습니다. : " + bro);
            foreach (LitJson.JsonData gameData in bro.FlattenRows())
            {
                ItemTable.Add(gameData["ItemName"].ToString(), int.Parse(gameData["ItemPrice"].ToString()));
            }
        }
    }
}