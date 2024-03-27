using System.Globalization;
using CustomEnumControl;
using DataControl;
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
        private Vector3 _initPos;
        private TowerData _towerData;
        private Sequence _openCardSequence;
        private TMP_Text _hpText;
        private TMP_Text _atkText;
        private TMP_Text _coolTimeText;
        private CanvasGroup _canvasGroup;

        public bool isOpen { get; private set; }

        [SerializeField] private RectTransform descriptionCardRect;
        [SerializeField] private TMP_Text towerNameText;
        [SerializeField] private TMP_Text towerDescriptionText;

        [SerializeField] private GameObject atkObj;
        [SerializeField] private GameObject healthObj;
        [SerializeField] private GameObject coolTimeObj;
        [SerializeField] private GameObject towerStatusPanel;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.blocksRaycasts = false;
            transform.GetChild(0).GetComponent<Button>().onClick.AddListener(CloseCard);
            _openCardSequence = DOTween.Sequence().SetAutoKill(false).Pause().SetUpdate(true)
                .Append(_canvasGroup.DOFade(1, 0.25f).From(0))
                .Join(descriptionCardRect.DOAnchorPosX(300, 0.25f).From(new Vector2(200, 0)));
            _openCardSequence.OnRewind(() => _canvasGroup.blocksRaycasts = false);
            _openCardSequence.OnComplete(() => _canvasGroup.blocksRaycasts = true);

            _atkText = atkObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _hpText = healthObj.transform.GetChild(1).GetComponent<TMP_Text>();
            _coolTimeText = coolTimeObj.transform.GetChild(1).GetComponent<TMP_Text>();
        }

        public void OpenTowerCard(TowerData towerData)
        {
            BuildTowerManager.DeSelectTower();
            if (towerData.Equals(_towerData)) return;
            isOpen = true;

            _towerData = towerData;

            towerNameText.text = TowerDataManager.TowerInfoTable[towerData.towerType].towerName;
            towerDescriptionText.text = TowerDataManager.TowerInfoTable[towerData.towerType].towerDescription;
            healthObj.SetActive(towerData.isUnitTower);
            coolTimeObj.SetActive(!towerData.isUnitTower);

            if (towerData is AttackTowerData battleTowerData)
            {
                towerStatusPanel.SetActive(true);
                if (battleTowerData.isUnitTower)
                {
                    var unitTowerData = (SummoningTowerData)battleTowerData;
                    _hpText.text = unitTowerData.curUnitHealth.ToString();
                }
                else
                {
                    _coolTimeText.text = battleTowerData.attackCooldown.ToString(CultureInfo.InvariantCulture);
                }

                _atkText.text = battleTowerData.curDamage.ToString();
            }
            else
            {
                towerStatusPanel.SetActive(false);
            }

            _openCardSequence.Restart();
        }

        public void CloseCard()
        {
            isOpen = false;

            _openCardSequence.PlayBackwards();
        }
    }
}