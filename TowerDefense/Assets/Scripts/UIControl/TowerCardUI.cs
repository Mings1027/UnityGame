using DataControl;
using DG.Tweening;
using GameControl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

        private TMP_Text _healthText;
        private TMP_Text _damageText;
        private TMP_Text _delayText;

        public bool IsOpen { get; private set; }

        [SerializeField] private TMP_Text towerNameText;
        [SerializeField] private TMP_Text towerDescriptionText;

        [SerializeField] private Image damageImage;
        [SerializeField] private GameObject healthObj;
        [SerializeField] private GameObject delayObj;
        [SerializeField] private GameObject towerStatusPanel;

        private void Awake()
        {
            _initPos = transform.position;
            _openCardSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(transform.DOScale(1, 0.25f).From(0))
                .Join(transform.GetChild(0).DORotate(new Vector3(0, 360, 0), 0.25f, RotateMode.FastBeyond360));
            _moveCardTween = transform.DOMove(_initPos, 0.25f).From().SetAutoKill(false);

            _healthText = healthObj.transform.GetChild(0).GetComponent<TMP_Text>();
            _damageText = damageImage.transform.GetChild(0).GetComponent<TMP_Text>();
            _delayText = delayObj.transform.GetChild(0).GetComponent<TMP_Text>();
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

                healthObj.SetActive(towerData.IsUnitTower);
                delayObj.SetActive(!towerData.IsUnitTower);

                if (towerData is BattleTowerData battleTowerData)
                {
                    towerStatusPanel.SetActive(true);
                    if (battleTowerData.IsUnitTower)
                    {
                        var unitTowerData = (UnitTowerData)battleTowerData;
                        _healthText.text = unitTowerData.UnitHealth.ToString();
                    }
                    else
                    {
                        _delayText.text = battleTowerData.AttackRpm.ToString();
                    }

                    damageImage.sprite = uiManager.GetTowerType(_towerData);
                    _damageText.text = battleTowerData.BaseDamage.ToString();
                }
                else
                {
                    towerStatusPanel.SetActive(false);
                }
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
            if (_towerData == null) return;
            var uiManager = UIManager.Instance;
            towerNameText.text = uiManager.towerNameDic[_towerData.TowerType];
            towerDescriptionText.text = uiManager.towerInfoDic[_towerData.TowerType];
        }
    }
}