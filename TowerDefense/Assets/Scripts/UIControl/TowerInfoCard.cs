using System.Globalization;
using CustomEnumControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using TMPro;
using TowerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerInfoCard : MonoBehaviour
    {
        private Image[] _starImages;
        private TowerType _towerType;
        private sbyte _prevTowerLevel;
        private Vector3 _towerPos;
        private bool _isTargeting;
        private Camera _cam;
        private Transform _towerTransform;
        private Tween _towerDisappearTween;
        private Tweener _towerCardMoveTween;
        private CanvasGroup _towerCardGroup;

        private TMP_Text _atkText;
        private TMP_Text _healthText;
        private TMP_Text _rangeText;
        private TMP_Text _coolTimeText;
        private TMP_Text _respawnText;

        [SerializeField] private RectTransform towerInfoCardRect;

        [SerializeField] private RectTransform starParent;
        [SerializeField] private GameObject atkObj;
        [SerializeField] private GameObject healthObj;
        [SerializeField] private GameObject rangeObj;
        [SerializeField] private GameObject coolTimeObj;
        [SerializeField] private GameObject respawnObj;
        
        [SerializeField] private GameObject towerLevelStar;
        [SerializeField] private CanvasGroup towerStatusPanelObj;

        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI sellGoldText;

        private void LateUpdate()
        {
            if (!_isTargeting) return;

            var towerPos = _cam.WorldToScreenPoint(_towerTransform.position);
            if (towerPos.x > Screen.width * 0.5f)
            {
                _towerCardMoveTween.ChangeStartValue(towerInfoCardRect.anchoredPosition)
                    .ChangeEndValue(new Vector2(-400, 0)).Restart();
            }
            else
            {
                _towerCardMoveTween.ChangeStartValue(towerInfoCardRect.anchoredPosition)
                    .ChangeEndValue(new Vector2(400, 0)).Restart();
            }
        }

        public void Init()
        {
            _cam = Camera.main;
            _prevTowerLevel = -1;
            _towerType = TowerType.None;
            _starImages = new Image[starParent.childCount];
            _towerCardGroup = GetComponent<CanvasGroup>();

            for (var i = 0; i < _starImages.Length; i++)
            {
                _starImages[i] = starParent.GetChild(i).GetChild(2).GetComponent<Image>();
            }

            _atkText = atkObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _healthText = healthObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _rangeText = rangeObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _coolTimeText = coolTimeObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _respawnText = respawnObj.transform.GetChild(1).GetComponent<TMP_Text>();

            _towerDisappearTween = _towerCardGroup.DOFade(1, 0.2f).From(0).SetAutoKill(false).Pause().SetUpdate(true);
            _towerDisappearTween.OnComplete(() => _towerCardGroup.blocksRaycasts = true);
            _towerDisappearTween.OnRewind(() => towerInfoCardRect.anchoredPosition = Vector2.zero);
            _towerCardMoveTween = towerInfoCardRect.DOAnchorPosX(towerInfoCardRect.anchoredPosition.x, 0.05f)
                .SetAutoKill(false).Pause().SetUpdate(true);
            _towerCardGroup.blocksRaycasts = false;
        }

        public void SetCardPos(bool value, Transform towerTransform)
        {
            _isTargeting = value;
            _towerTransform = towerTransform;
        }

        public void OpenCard()
        {
            _towerDisappearTween.Restart();
        }

        public void CloseCard()
        {
            _towerCardGroup.blocksRaycasts = false;
            _towerDisappearTween.PlayBackwards();
        }

        public void SetTowerInfo(AttackTower tower, TowerData towerData, sbyte level,
            ushort upgradeCost, ushort sellCost, string towerName)
        {
            towerStatusPanelObj.alpha = 1;
            towerLevelStar.SetActive(true);

            if (towerData is AttackTowerData battleTowerData)
            {
                if (towerData.isUnitTower)
                {
                    var summonTowerData = (SummoningTowerData)towerData;
                    _healthText.text = (summonTowerData.curUnitHealth * (level + 1)).ToString();
                    _respawnText.text = summonTowerData.initReSpawnTime.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    _coolTimeText.text = battleTowerData.attackCooldown.ToString(CultureInfo.InvariantCulture);
                }
            }

            var isUnitTower = towerData.isUnitTower;
            healthObj.SetActive(isUnitTower);
            rangeObj.SetActive(!isUnitTower);
            coolTimeObj.SetActive(!isUnitTower);
            respawnObj.SetActive(isUnitTower);
            var towerType = tower.towerType;

            if (!towerType.Equals(_towerType))
            {
                _towerType = towerType;
                towerNameText.text = towerName;
            }

            DisplayStarsForTowerLevel(level);
            goldText.text = upgradeCost + "G";
            _atkText.text = tower.towerDamage.ToString();
            _rangeText.text = tower.towerRange.ToString();
            sellGoldText.text = sellCost + "G";
        }

        public void SetSupportTowerInfo(SupportTower tower, ushort sellCost, string towerName)
        {
            towerStatusPanelObj.alpha = 0;
            towerLevelStar.SetActive(false);

            var towerType = tower.towerType;

            if (!towerType.Equals(_towerType))
            {
                _towerType = towerType;
                towerNameText.text = towerName;
            }

            sellGoldText.text = sellCost + "G";
        }

        private void DisplayStarsForTowerLevel(sbyte level)
        {
            if (_prevTowerLevel == level) return;
            _prevTowerLevel = level;

            for (var i = 0; i < starParent.childCount; i++)
            {
                _starImages[i].enabled = i <= level;
            }
        }

    }
}