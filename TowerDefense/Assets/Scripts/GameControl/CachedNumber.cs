using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace GameControl
{
    public abstract class CachedNumber
    {
        private static readonly Dictionary<ushort, string> CachedFloatingTextDictionary = new();
        private static readonly Dictionary<float, string> CachedUITextDictionary = new();
        // private static readonly Dictionary<ushort, string> CachedCostTextDictionary = new();

        public static string GetFloatingText(ushort value, bool isGainedCoin)
        {
            if (CachedFloatingTextDictionary.TryGetValue(value, out var cached))
                return isGainedCoin ? "+" + cached : "-" + cached;
            var floatingText = value.ToString();
            CachedFloatingTextDictionary.Add(value, floatingText);
            return isGainedCoin ? "+" + floatingText : "-" + floatingText;
        }

        public static string GetUIText(float value)
        {
            if (CachedUITextDictionary.TryGetValue(value, out var cached))
                return cached;
            CachedUITextDictionary.Add(value, value.ToString(CultureInfo.CurrentCulture));
            return CachedUITextDictionary[value];
        }

        // public static string GetCostUIText(ushort value)
        // {
        //     if (CachedCostTextDictionary.TryGetValue(value, out var cached))
        //         return cached;
        //     var costText = value + "g";
        //     CachedCostTextDictionary.Add(value, costText);
        //     return costText;
        // }
    }
}