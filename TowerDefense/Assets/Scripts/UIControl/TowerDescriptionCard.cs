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
        private Vector2 _buttonPos;
        private Vector3 _initPos;
        private TowerType _towerType;
        private Tween _openCardTween;
        private Tweener _moveCardTween;
        private TMP_Text _healthText;
        private TMP_Text _damageText;
        private TMP_Text _delayText;
        private CanvasGroup _canvasGroup;
        private RectTransform _towerCardRect;

        public bool isOpen { get; private set; }

        [SerializeField] private TMP_Text towerNameText;
        [SerializeField] private TMP_Text towerDescriptionText;

        [SerializeField] private Image damageImage;
        [SerializeField] private GameObject healthObj;
        [SerializeField] private GameObject delayObj;
        [SerializeField] private GameObject towerStatusPanel;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _towerCardRect = transform.GetChild(1).GetComponent<RectTransform>();
            _canvasGroup.blocksRaycasts = false;
            transform.GetChild(0).GetComponent<Button>().onClick.AddListener(CloseCard);
            _towerType = TowerType.None;

            _openCardTween = _canvasGroup.DOFade(1, 0.25f).From(0).SetAutoKill(false).Pause();

            _moveCardTween = _towerCardRect.DOAnchorPos(Vector2.zero, 0.25f).From(Vector2.zero)
                .SetAutoKill(false).Pause();
            _moveCardTween.OnComplete(() => _canvasGroup.blocksRaycasts = true);
            _moveCardTween.OnRewind(() => _canvasGroup.blocksRaycasts = false);

            _healthText = healthObj.transform.GetChild(0).GetComponent<TMP_Text>();
            _damageText = damageImage.transform.GetChild(0).GetComponent<TMP_Text>();
            _delayText = delayObj.transform.GetChild(0).GetComponent<TMP_Text>();
        }

        public void OpenTowerCard(TowerType towerType, RectTransform buttonTransform)
        {
            if (towerType.Equals(_towerType)) return;

            isOpen = true;
            _buttonPos = buttonTransform.anchoredPosition;

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

            _openCardTween.Restart();
            _moveCardTween.ChangeStartValue(_buttonPos + new Vector2(0, -100)).ChangeEndValue(_buttonPos).Restart();
        }

        public void CloseCard()
        {
            isOpen = false;
            _towerType = TowerType.None;

            _openCardTween.PlayBackwards();
            _moveCardTween.PlayBackwards();
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