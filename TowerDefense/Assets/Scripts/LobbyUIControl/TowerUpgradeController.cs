using System.Linq;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl.TowerDataControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class TowerUpgradeController : MonoBehaviour
    {
        private LobbyUI _lobbyUI;
        private Tween _upgradePanelTween;
        private Tween _deletePanelTween;

        [SerializeField] private TMP_Text xpText;
        [SerializeField] private Image blockImage;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button initLevelButton;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private Transform upgradePanel;

        [SerializeField] private Image deletePanelBlockImage;
        [SerializeField] private Transform deletePanel;

        [SerializeField] private Transform towerButtons;
        [SerializeField] private AttackTowerData[] towerData;
        private const byte TowerMaxLevel = 20;

        private void Awake()
        {
            _lobbyUI = FindAnyObjectByType<LobbyUI>();
            _upgradePanelTween =
                upgradePanel.DOScaleX(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            _deletePanelTween = deletePanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause()
                .SetUpdate(true);
            Input.multiTouchEnabled = false;
            blockImage.enabled = false;
            deletePanelBlockImage.enabled = false;
            ButtonInit();
        }

        private void Start()
        {
            xpText.text = "XP " + BackendGameData.userData.xp;
            TowerUpgradeButtonInit();
        }

        private void OnDestroy()
        {
            _upgradePanelTween?.Kill();
            _deletePanelTween?.Kill();
        }

        private void UpgradePanel()
        {
            upgradeButton.gameObject.SetActive(false);
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = true;
            _upgradePanelTween.Restart();
        }

        private void ButtonInit()
        {
            startGameButton.onClick.AddListener(() => { SceneManager.LoadScene("MainGameScene"); });
            upgradeButton.onClick.AddListener(() =>
            {
                UpgradePanel();
                _lobbyUI.SetActiveButtons(false, false);
            });
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                blockImage.enabled = false;
                upgradeButton.gameObject.SetActive(true);
                _upgradePanelTween.PlayBackwards();
                _lobbyUI.SetActiveButtons(true, false);
            });
            initLevelButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                deletePanelBlockImage.enabled = true;
                _deletePanelTween.Restart();
            });
            yesButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                InitTowerLevel();
            });
            noButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                deletePanelBlockImage.enabled = false;
                _deletePanelTween.PlayBackwards();
            });
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
            deletePanelBlockImage.enabled = false;
            _deletePanelTween.PlayBackwards();

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
                // towerData[i].InitState();
            }

            foreach (var key in BackendGameData.userData.towerLevelTable.Keys.ToList())
            {
                BackendGameData.userData.towerLevelTable[key] = 0;
            }

            initLevelButton.interactable = false;
            UniTask.RunOnThreadPool(() =>
            {
                BackendGameData.instance.GameDataUpdate();

                initLevelButton.interactable = true;
            });
        }
    }
}