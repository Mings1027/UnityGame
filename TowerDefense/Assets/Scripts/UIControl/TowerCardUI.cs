using DataControl;
using DG.Tweening;
using GameControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerCardUI : MonoBehaviour
    {
        private Vector3 _buttonPos;
        private Vector3 _initPos;
        private TowerData _towerData;
        private Sequence _openCardSequence;
        private Tweener _moveCardTween;

        public bool IsOpen { get; private set; }

        [SerializeField] private TMP_Text towerNameText;
        [SerializeField] private TMP_Text towerDescriptionText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text damageText;
        [SerializeField] private TMP_Text delayText;

        [SerializeField] private GameObject healthImage;
        [SerializeField] private Image damageImage;
        [SerializeField] private Image delayImage;

        private void Awake()
        {
            _initPos = transform.position;
            _openCardSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(transform.DOScale(1, 0.25f).From(0))
                .Join(transform.GetChild(0).DORotate(new Vector3(0, 360, 0), 0.25f, RotateMode.FastBeyond360));
            _moveCardTween = transform.DOMove(_initPos, 0.25f).From().SetAutoKill(false);
        }

        public void OpenTowerCard(TowerData towerData, Transform buttonTransform)
        {
            IsOpen = true;
            _buttonPos = buttonTransform.position;

            if (!towerData.Equals(_towerData))
            {
                _towerData = towerData;
                var uiManager = UIManager.Instance;

                towerNameText.text = uiManager.towerNameDic[towerData.TowerType];
                towerDescriptionText.text = uiManager.towerInfoDic[towerData.TowerType];

                healthImage.SetActive(towerData.IsUnitTower);
                if (towerData.IsUnitTower)
                {
                    var unitTowerData = (UnitTowerData)towerData;
                    healthText.text = CachedNumber.GetUIText(unitTowerData.UnitHealth);
                    delayText.text = CachedNumber.GetUIText(unitTowerData.UnitReSpawnTime);
                }
                else
                {
                    delayText.text = CachedNumber.GetUIText(towerData.AttackRpm);
                }

                damageImage.sprite = uiManager.GetTowerType(_towerData);
                delayImage.sprite = uiManager.IsUnitTower(towerData);
                damageText.text = CachedNumber.GetUIText(towerData.BaseDamage);
            }

            _openCardSequence.Restart();
            _moveCardTween.ChangeStartValue(_buttonPos)
                .ChangeEndValue(_buttonPos + new Vector3(0, 450, 0)).Restart();
        }

        public void CloseCard()
        {
            IsOpen = false;

            _openCardSequence.PlayBackwards();
            _moveCardTween.ChangeStartValue(_buttonPos + new Vector3(0, 450, 0)).ChangeEndValue(_buttonPos).Restart();
        }

        public void LocaleCardInfo()
        {
            var uiManager = UIManager.Instance;
            towerNameText.text = uiManager.towerNameDic[_towerData.TowerType];
            towerDescriptionText.text = uiManager.towerInfoDic[_towerData.TowerType];
        }
    }
}