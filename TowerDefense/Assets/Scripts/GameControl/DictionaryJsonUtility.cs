using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameControl
{
    [Serializable]
    public class DataDictionary<TKey, TValue>
    {
        public TKey key;
        public TValue value;
    }

    [Serializable]
    public class JsonDataArray<TKey, TValue>
    {
        public List<DataDictionary<TKey, TValue>> data;
    }

    public static class DictionaryJsonUtility
    {
        public static string ToJson<TKey, TValue>(Dictionary<TKey, TValue> jsonDicData, bool pretty = false)
        {
            var dataList = jsonDicData.Keys.Select(key => new DataDictionary<TKey, TValue>
                { key = key, value = jsonDicData[key] }).ToList();

            var arrayJson = new JsonDataArray<TKey, TValue>
            {
                data = dataList
            };
            return JsonUtility.ToJson(arrayJson, pretty);
        }

        public static Dictionary<TKey, TValue> FromJson<TKey, TValue>(string jsonData)
        {
            var dataList = JsonUtility.FromJson<JsonDataArray<TKey, TValue>>(jsonData);
            var returnDictionary = new Dictionary<TKey, TValue>();
            for (var i = 0; i < dataList.data.Count; i++)
            {
                var dictionaryData = dataList.data[i];
                returnDictionary[dictionaryData.key] = dictionaryData.value;
            }

            return returnDictionary;
        }
    }
}