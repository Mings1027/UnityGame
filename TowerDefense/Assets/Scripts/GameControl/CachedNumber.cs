using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace GameControl
{
    public abstract class CachedNumber
    {
        private static readonly Dictionary<ushort, string> CachedFloatingTextDictionary = new();
        private static readonly Dictionary<float, string> CachedUITextDictionary = new();

        public static string GetFloatingText(ushort value)
        {
            if (CachedFloatingTextDictionary.TryGetValue(value, out var cached))
                return cached;
            var floatingText = value.ToString();
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
    }
}