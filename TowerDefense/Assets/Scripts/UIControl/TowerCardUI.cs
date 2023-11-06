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
        private Vector3 buttonPos;
        private Vector3 initPos;
        private const string CardKey = "Card-";
        private Locale prevLanguage;
        private TowerData _towerData;

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
            initPos = transform.position;
        }

        private void Start()
        {
            _canvas.enabled = false;
        }

        public void SetTowerCardInfo(Vector3 from, TowerData towerData)
        {
            _canvas.enabled = true;
            buttonPos = from;

            DOTween.Sequence().Append(transform.DOScale(1, 0.25f).From(0))
                .Join(transform.DOMove(initPos, 0.25f).From(buttonPos))
                .Join(transform.GetChild(0).DORotate(new Vector3(0, 360, 0), 0.25f, RotateMode.FastBeyond360));

            if (towerData.Equals(_towerData) && LocalizationSettings.SelectedLocale.Equals(prevLanguage)) return;

            _towerData = towerData;
            prevLanguage = LocalizationSettings.SelectedLocale;

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

        public async UniTaskVoid DisableCard()
        {
            await DOTween.Sequence()
                .Append(transform.GetChild(0).DORotate(new Vector3(0, -360, 0), 0.25f, RotateMode.FastBeyond360))
                .Join(transform.DOMove(buttonPos, 0.25f))
                .Join(transform.DOScale(0, 0.25f));

            _canvas.enabled = false;
        }
    }
}