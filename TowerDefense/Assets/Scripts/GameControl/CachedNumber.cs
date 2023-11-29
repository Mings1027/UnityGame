using System.Collections.Generic;
using System.Globalization;

namespace GameControl
{
    public abstract class CachedNumber
    {
        private static readonly Dictionary<int, string> CachedFloatingTextDictionary = new();
        private static readonly Dictionary<float, string> CachedUITextDictionary = new();
        private static readonly Dictionary<int, string> CachedCostTextDictionary = new();

        public static string GetFloatingText(int value)
        {
            if (CachedFloatingTextDictionary.TryGetValue(value, out var cached))
                return cached;
            var floatingText = "+" + value;
            CachedFloatingTextDictionary.Add(value, floatingText);
            return floatingText;
        }

        public static string GetUIText(float value)
        {
            if (CachedUITextDictionary.TryGetValue(value, out var cached))
                return cached;
            CachedUITextDictionary.Add(value, value.ToString(CultureInfo.CurrentCulture));
            return CachedUITextDictionary[value];
        }

        public static string GetCostText(int value)
        {
            if (CachedCostTextDictionary.TryGetValue(value, out var cached))
                return cached;
            var costText = value + "g";
            CachedCostTextDictionary.Add(value, costText);
            return costText;
        }
    }
}