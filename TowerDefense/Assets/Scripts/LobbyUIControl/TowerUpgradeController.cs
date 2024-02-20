using System.Linq;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UIControl;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class TowerUpgradeController : MonoBehaviour
    {
        private LobbyUI _lobbyUI;
        private Tween _upgradePanelGroupSequence;
        private Tween _deletePanelTween;
        private CanvasGroup _upgradePanelGroup;
        private const byte TowerMaxLevel = 20;

        [SerializeField] private TMP_Text xpText;

        [SerializeField] private RectTransform upgradePanel;
        [SerializeField] private Button upgradeButton;

        [SerializeField] private Button closeButton;
        [SerializeField] private NoticePanel notifyInitLevelPanel;

        [SerializeField] private Transform towerButtons;

        private void Awake()
        {
            _upgradePanelGroup = upgradePanel.GetComponent<CanvasGroup>();
            _lobbyUI = GetComponentInParent<LobbyUI>();
            _upgradePanelGroupSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_upgradePanelGroup.DOFade(1, 0.25f).From(0))
                .Join(upgradePanel.DOAnchorPosY(0, 0.25f).From(new Vector2(0, -100)));
            _upgradePanelGroupSequence.OnComplete(() => _upgradePanelGroup.blocksRaycasts = true);
            _upgradePanelGroupSequence.OnRewind(() =>
            {
                _lobbyUI.OffBlockImage();
                _upgradePanelGroup.blocksRaycasts = false;
            });
            _upgradePanelGroup.blocksRaycasts = false;
            Input.multiTouchEnabled = false;
            ButtonInit();
        }

        private void Start()
        {
            xpText.text = "XP " + BackendGameData.userData.xp;
            TowerUpgradeButtonInit();
        }

        private void OnDestroy()
        {
            _upgradePanelGroupSequence?.Kill();
            _deletePanelTween?.Kill();
        }

        private void UpgradePanel()
        {
            upgradeButton.gameObject.SetActive(false);
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _lobbyUI.OnBackgroundImage();
            _upgradePanelGroupSequence.Restart();
        }

        private void ButtonInit()
        {
            upgradeButton.onClick.AddListener(() =>
            {
                UpgradePanel();
                _lobbyUI.SetActiveButtons(false, false);
            });
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                upgradeButton.gameObject.SetActive(true);
                _lobbyUI.OffBackgroundImage();
                _lobbyUI.SetActiveButtons(true, false);
                _upgradePanelGroupSequence.PlayBackwards();
            });
            notifyInitLevelPanel.OnConfirmButtonEvent += InitTowerLevel;
            notifyInitLevelPanel.OnCancelButtonEvent += () => SoundManager.PlayUISound(SoundEnum.ButtonSound);
        }

        private void TowerUpgradeButtonInit()
        {
            var towerLevelTable = BackendGameData.userData.towerLevelTable;
            for (var i = 0; i < towerButtons.childCount; i++)
            {
                var towerButtonParent = towerButtons.GetChild(i);
                var levelCountText = towerButtonParent.GetChild(0).GetComponent<TMP_Text>();
                var towerCostText = towerButtonParent.GetChild(1).GetComponent<TMP_Text>();
                var towerUpgradeButton = towerButtonParent.GetChild(2).GetComponent<TowerUpgradeButton>();
                var towerButton = towerUpgradeButton.GetComponent<Button>();

                var towerLevel = towerLevelTable[towerUpgradeButton.attackTowerData.towerType.ToString()];
                if (towerLevel < TowerMaxLevel)
                {
                    towerCostText.text = "XP " + (towerLevel + 1) * 25;
                    levelCountText.text = "Lv " + towerLevel + " / " + TowerMaxLevel;
                }
                else
                {
                    towerCostText.text = "";
                    levelCountText.text = "Lv " + TowerMaxLevel + " / " + TowerMaxLevel;
                    towerButton.interactable = false;
                }

                towerButton.onClick.AddListener(() =>
                {
                    SoundManager.PlayUISound(SoundEnum.ButtonSound);
                    var userData = BackendGameData.userData;
                    var towerType = towerUpgradeButton.attackTowerData.towerType.ToString();
                    var prevTowerLv = towerLevelTable[towerType];

                    if (prevTowerLv >= TowerMaxLevel || userData.xp < (prevTowerLv + 1) * 25) return;
                    userData.xp -= (prevTowerLv + 1) * 25;
                    xpText.text = "XP " + userData.xp;
                    userData.towerLevelTable[towerType] += 1;

                    var curTowerLv = userData.towerLevelTable[towerType];

                    towerCostText.text = "XP " + (curTowerLv + 1) * 25;
                    levelCountText.text = "Lv " + curTowerLv + " / " + TowerMaxLevel;
                    towerUpgradeButton.attackTowerData.UpgradeData(curTowerLv);

                    if (towerLevel >= TowerMaxLevel)
                    {
                        towerCostText.text = "";
                        towerButton.interactable = false;
                    }
                });
            }
        }

        private int GetSpentXp()
        {
            var spentXp = BackendGameData.userData.xp;
            var towerLevelTable = BackendGameData.userData.towerLevelTable;
            foreach (var key in towerLevelTable.Keys)
            {
                var towerLv = towerLevelTable[key];
                if (towerLv <= 0) continue;
                for (var i = 0; i < towerLv; i++)
                {
                    spentXp += (i + 1) * 25;
                }
            }

            return spentXp;
        }

        private void InitTowerLevel()
        {
            if (BackendGameData.userData.xp <= 0) return;

            var xp = BackendGameData.userData.xp = GetSpentXp();
            xpText.text = "XP " + xp;

            for (var i = 0; i < towerButtons.childCount; i++)
            {
                var levelCountText = towerButtons.GetChild(i).GetChild(0).GetComponent<TMP_Text>();
                var towerCostText = towerButtons.GetChild(i).GetChild(1).GetComponent<TMP_Text>();
                var towerUpgradeButton = towerButtons.GetChild(i).GetChild(2).GetComponent<TowerUpgradeButton>();
                var towerButton = towerUpgradeButton.GetComponent<Button>();

                towerButton.interactable = true;
                towerCostText.text = "XP 25";
                levelCountText.text = "Lv 0 / " + TowerMaxLevel;
            }

            foreach (var key in BackendGameData.userData.towerLevelTable.Keys.ToList())
            {
                BackendGameData.userData.towerLevelTable[key] = 0;
            }

            UniTask.RunOnThreadPool(() => { BackendGameData.instance.GameDataUpdate(); });
        }
    }
}