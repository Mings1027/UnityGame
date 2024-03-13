using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ManagerControl
{
    public abstract class LocaleManager
    {
        private static bool _isChanging;

        public const string TowerCardTable = "TowerCard Table";
        public const string ItemTable = "Item Table";
        public const string LobbyUITable = "Lobby UI Table";
        public const string FullscreenAlertTable = "Fullscreen Alert Table";
        public const string FloatingNotifyTable = "Floating Notify Table";
        public const string CardKey = "Card-";
        public const string ItemKey = "Item-";

        public static void ChangeLocale(int index)
        {
            if (_isChanging) return;

            ChangeLanguage(index).Forget();
        }

        private static async UniTaskVoid ChangeLanguage(int index)
        {
            _isChanging = true;

            await LocalizationSettings.InitializationOperation;

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];

            _isChanging = false;
        }

        public static async UniTaskVoid ChangeLocaleAsync(string tableName, string key, TMP_Text textToChange)
        {
            var loadingOperation = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
            await loadingOperation;
            if (loadingOperation.Status == AsyncOperationStatus.Succeeded)
            {
                var table = loadingOperation.Result;
                var localizedString = table.GetEntry(key)?.GetLocalizedString();

                textToChange.text = localizedString;
            }
        }

        public static string GetLocalizedString(string tableName, string inputString)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(tableName, inputString,
                LocalizationSettings.SelectedLocale);
        }
    }
}