#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Utilities
{
    public abstract class UtilityClass
    {
        [MenuItem("GameObject/UI Splitter")]
        private static void UISplitter()
        {
            var verticals = Object.FindObjectsByType<UIVerticalSplitter>(FindObjectsSortMode.InstanceID);
            for (int i = 0; i < verticals.Length; i++)
            {
                verticals[i].VerticalSplitter();
            }

            var horizontals = Object.FindObjectsByType<UIHorizontalSplitter>(FindObjectsSortMode.InstanceID);
            for (int i = 0; i < horizontals.Length; i++)
            {
                horizontals[i].HorizontalSplitter();
            }
        }
    }
}
#endif