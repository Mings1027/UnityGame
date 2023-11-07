using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerCardUI : MonoBehaviour
    {
        private Canvas _canvas;
        private Vector3 _buttonPos;
        private Vector3 _initPos;
        private Locale _prevLanguage;
        private TowerData _towerData;
        private Sequence _cardFloatingSequence;
        private const string CardKey = "Card-";

        [SerializeField] private TMP_Text towerNameText;
        [SerializeField] private TMP_Text towerDescriptionText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text damageText;
        [SerializeField] private TMP_Text attackDelayText;

        [SerializeField] private GameObject healthImage;
        [SerializeField] private Image damageImage;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _initPos = transform.position;
            var rectTransform = GetComponent<RectTransform>();
            _cardFloatingSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(rectTransform.DOAnchorPosY(10, 1).From(new Vector2(-10, 0)).SetEase(Ease.InOutSine));
        }

        private void Start()
        {
            _canvas.enabled = false;
        }

        public async UniTaskVoid OpenTowerCard(Vector3 from, TowerData towerData)
        {
            _canvas.enabled = true;
            _buttonPos = from;

            if (!towerData.Equals(_towerData) || !LocalizationSettings.SelectedLocale.Equals(_prevLanguage))
            {
                _towerData = towerData;
                _prevLanguage = LocalizationSettings.SelectedLocale;

                towerNameText.text = LocaleManager.GetLocalizedString(towerData.TowerType.ToString());

                towerDescriptionText.text = LocaleManager.GetLocalizedString(CardKey + towerData.TowerType);

                healthImage.SetActive(towerData.IsUnitTower);
                if (towerData.IsUnitTower)
                {
                    var unitTowerData = (UnitTowerData)towerData;
                    healthText.text = unitTowerData.UnitHealth.ToString();
                }

                damageImage.sprite = UIManager.Instance.WhatTypeOfThisTower(_towerData);

                var towerLevelData = towerData.TowerLevels[0];
                damageText.text = towerLevelData.damage.ToString();
                attackDelayText.text = towerLevelData.attackDelay.ToString(CultureInfo.InvariantCulture);
            }

            await DOTween.Sequence().Append(transform.DOScale(1, 0.25f).From(0))
                .Join(transform.DOMove(_initPos, 0.25f).From(_buttonPos))
                .Join(transform.GetChild(0).DORotate(new Vector3(0, 360, 0), 0.25f, RotateMode.FastBeyond360));

            _cardFloatingSequence.SetLoops(-1, LoopType.Yoyo).Restart();
        }

        public async UniTaskVoid DisableCard()
        {
            _cardFloatingSequence.Pause();
            await DOTween.Sequence()
                .Append(transform.DOScale(0, 0.25f))
                .Join(transform.DOMove(_buttonPos, 0.25f))
                .Join(transform.GetChild(0).DORotate(new Vector3(0, 180, 0), 0.25f));

            _canvas.enabled = false;
        }
    }
}