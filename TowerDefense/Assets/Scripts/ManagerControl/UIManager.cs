using System;
using System.Collections.Generic;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using GameControl;
using InterfaceControl;
using MapControl;
using TMPro;
using UIControl;
using UnityEngine;

namespace ManagerControl
{
    public class UIManager : MonoSingleton<UIManager>, IMainGameObject
    {
#region Private Variable

        private TowerCardController _towerCardController;

        private Dictionary<TowerType, TMP_Text> _towerGoldTextDictionary;

        private TMP_Text[] _towerGoldTexts;

        [SerializeField] private CanvasGroup toggleTowerBtnImage;

#endregion

#region Unity Event

        private void OnDisable()
        {
            TowerDataManager.RemoveLocaleMethod();
        }

#endregion

#region Init

        public void Init()
        {
            UIManagerInit();
            TowerButtonInit();
            TowerInit();

            Input.multiTouchEnabled = false;
            GameHUD.SetWaveText("0");
            Time.timeScale = 1;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            SoundManager.PlayBGM(SoundEnum.WaveEnd);
        }

        private void UIManagerInit()
        {
            _towerCardController = FindAnyObjectByType<TowerCardController>();
            FindAnyObjectByType<GameHUD>().Init();
        }

        private void TowerButtonInit()
        {
            var towerButtons = _towerCardController.GetComponentsInChildren<TowerButton>();
            _towerGoldTexts = new TMP_Text[towerButtons.Length];

            for (var i = 0; i < towerButtons.Length; i++)
            {
                _towerGoldTexts[i] = towerButtons[i].GetComponentInChildren<TMP_Text>();
            }
        }

        private void TowerInit()
        {
            _towerGoldTextDictionary = new Dictionary<TowerType, TMP_Text>();

            var towerTypes = Enum.GetValues(typeof(TowerType));
            var index = 0;
            foreach (TowerType towerType in towerTypes)
            {
                if (towerType == TowerType.None) continue;
                _towerGoldTextDictionary.Add(towerType, _towerGoldTexts[index]);
                index++;
            }
        }

#endregion

#region Public Method

        public static void MapSelectButton(byte difficultyLevel)
        {
            instance.MapSelectButtonPrivate(difficultyLevel);
        }

        public static void AppearUI()
        {
            instance.AppearUIPrivate();
        }

        public static void SetTowerGoldText(TowerType towerType, string towerGoldText)
        {
            instance.SetTowerGoldTextPrivate(towerType, towerGoldText);
        }

#endregion

#region Private Method

        private async UniTaskVoid MapSelectButtonPrivate(byte difficultyLevel)
        {
            var mapManager = FindAnyObjectByType<MapManager>();
            mapManager.MakeMap(difficultyLevel);
            GameHUD.SetTowerGold(difficultyLevel);
            BackendGameData.instance.SetLevel(difficultyLevel);
            await UniTask.Delay(500, cancellationToken: this.GetCancellationTokenOnDestroy());

            GameHUD.DisplayHUD();
            await _towerCardController.GetComponent<RectTransform>().DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack)
                .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
            toggleTowerBtnImage.GetComponent<TutorialController>().TutorialButton();
            Input.multiTouchEnabled = true;
            CameraManager.isControlActive = true;
        }

        private void SetTowerGoldTextPrivate(TowerType towerType, string towerGoldText)
        {
            _towerGoldTextDictionary[towerType].text = towerGoldText;
        }

        private void AppearUIPrivate()
        {
            _towerCardController.AppearToggleButton();
            GameHUD.DisplayHUD();
        }

#endregion
    }
}