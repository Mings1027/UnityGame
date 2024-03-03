using System.Collections.Generic;
using BackEnd;
using UnityEngine;
using Utilities;

namespace BackendControl
{
    public class BackendChart
    {
        private static BackendChart _instance;
        public static BackendChart instance => _instance ??= new BackendChart();
        public static readonly Dictionary<string, int> ItemTable = new();

        public void ChartGet()
        {
            if (ItemTable.Count > 0) return;
            const string chartId = "106985";
            CustomLog.Log($"{chartId}의 차트 불러오기를 요청합니다.");
            var bro = Backend.Chart.GetChartContents(chartId);

            if (bro.IsSuccess() == false)
            {
                CustomLog.LogError($"{chartId}의 차트를 불러오는 중, 에러가 발생했습니다. : " + bro);
                return;
            }

            CustomLog.Log("차트 불러오기에 성공했습니다. : " + bro);
            foreach (LitJson.JsonData gameData in bro.FlattenRows())
            {
                ItemTable.Add(gameData["ItemName"].ToString(), int.Parse(gameData["ItemPrice"].ToString()));
            }
        }

        public void InitItemTable() => ItemTable.Clear();
    }
}