using System.Globalization;
using CustomEnumControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerDescriptionCard : MonoBehaviour
    {
        private Vector3 _buttonPos;
        private Vector3 _initPos;
        private TowerType _towerType;
        private Sequence _openCardSequence;
        private Tweener _moveCardTween;
        private GameObject _cardCloseButton;
        private TMP_Text _healthText;
        private TMP_Text _damageText;
        private TMP_Text _delayText;

        public bool isOpen { get; private set; }

        [SerializeField] private TMP_Text towerNameText;
        [SerializeField] private TMP_Text towerDescriptionText;

        [SerializeField] private Image damageImage;
        [SerializeField] private GameObject healthObj;
        [SerializeField] private GameObject delayObj;
        [SerializeField] private GameObject towerStatusPanel;

        private void Awake()
        {
            _cardCloseButton = transform.GetChild(0).gameObject;
            _cardCloseButton.SetActive(false);
            _towerType = TowerType.None;
            _initPos = transform.position;
            _openCardSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(transform.GetChild(1).DOScale(1, 0.25f).From(0))
                .Join(transform.GetChild(1).DORotate(new Vector3(0, 360, 0), 0.25f, RotateMode.FastBeyond360));
            _moveCardTween = transform.GetChild(1).DOMove(_initPos, 0.25f).From().SetAutoKill(false);

            _healthText = healthObj.transform.GetChild(0).GetComponent<TMP_Text>();
            _damageText = damageImage.transform.GetChild(0).GetComponent<TMP_Text>();
            _delayText = delayObj.transform.GetChild(0).GetComponent<TMP_Text>();
        }

        public void OpenTowerCard(TowerType towerType, Transform buttonTransform)
        {
            _cardCloseButton.SetActive(true);
            isOpen = true;
            _buttonPos = buttonTransform.position;

            if (!towerType.Equals(_towerType))
            {
                _towerType = towerType;
                var uiManager = UIManager.instance;

                towerNameText.text = uiManager.towerNameDic[towerType];
                towerDescriptionText.text = uiManager.towerInfoDic[towerType];
                var towerDataDic = uiManager.towerDataPrefabDictionary;
                healthObj.SetActive(towerDataDic[towerType].towerData.isUnitTower);
                delayObj.SetActive(!towerDataDic[towerType].towerData.isUnitTower);

                if (towerDataDic[towerType].towerData is AttackTowerData battleTowerData)
                {
                    towerStatusPanel.SetActive(true);
                    if (battleTowerData.isUnitTower)
                    {
                        var unitTowerData = (SummoningTowerData)battleTowerData;
                        _healthText.text = unitTowerData.curUnitHealth.ToString();
                    }
                    else
                    {
                        _delayText.text = battleTowerData.attackCooldown.ToString(CultureInfo.InvariantCulture);
                    }

                    damageImage.sprite = uiManager.GetTowerType(_towerType);
                    _damageText.text = battleTowerData.curDamage.ToString();
                }
                else
                {
                    towerStatusPanel.SetActive(false);
                }
            }

            gameObject.SetActive(true);
            _openCardSequence.Restart();
            _moveCardTween.ChangeStartValue(_buttonPos)
                .ChangeEndValue(_buttonPos + new Vector3(0, 450, 0)).Restart();
        }

        public void CloseCard()
        {
            _cardCloseButton.SetActive(false);
            isOpen = false;

            _openCardSequence.PlayBackwards();
            _moveCardTween.ChangeStartValue(_buttonPos + new Vector3(0, 450, 0)).ChangeEndValue(_buttonPos).Restart();
        }

        public void LocaleCardInfo()
        {
            var uiManager = UIManager.instance;
            if (_towerType == TowerType.None) return;
            towerNameText.text = uiManager.towerNameDic[_towerType];
            towerDescriptionText.text = uiManager.towerInfoDic[_towerType];
        }
    }
}