using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        var dataList = new List<DataDictionary<TKey, TValue>>();
        DataDictionary<TKey, TValue> dictionaryData;
        foreach (var key in jsonDicData.Keys)
        {
            dictionaryData = new DataDictionary<TKey, TValue>();
            dictionaryData.key = key;
            dictionaryData.value = jsonDicData[key];
            dataList.Add(dictionaryData);
        }

        var arrayJson = new JsonDataArray<TKey, TValue>();
        arrayJson.data = dataList;
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