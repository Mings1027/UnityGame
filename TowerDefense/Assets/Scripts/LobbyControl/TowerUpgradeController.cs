using System;
using BackendControl;
using CustomEnumControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities;

namespace LobbyControl
{
    public class TowerUpgradeController : MonoBehaviour
    {
        private Tween _upgradePanelTween;
        private Tween _deletePanelTween;
        private int _totalSpentXp;

        [SerializeField] private TMP_Text xpText;
        [SerializeField] private Image blockImage;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button dataDeleteButton;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private Transform upgradePanel;

        [SerializeField] private Image deletePanelBlockImage;
        [SerializeField] private Transform deletePanel;

        [SerializeField] private Transform towerButtons;
        [SerializeField] private AttackTowerData[] towerData;
        [SerializeField] private byte towerMaxLevel;

        [SerializeField] private GameObject buttons;

        private void Awake()
        {
            _upgradePanelTween =
                upgradePanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            _deletePanelTween = deletePanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause()
                .SetUpdate(true);
            Input.multiTouchEnabled = false;
            blockImage.enabled = false;
            deletePanelBlockImage.enabled = false;
            ButtonInit();
        }

        private void Start()
        {
            _totalSpentXp = BackendGameData.userData.totalSpentXp;
            // DataManager.xp = PlayerPrefs.GetInt(StringManager.Xp);
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
            startGameButton.onClick.AddListener(() =>
            {
                // BackendGameData.instance.GameDataUpdate();
                SceneManager.LoadScene("MainGameScene");
            });
            upgradeButton.onClick.AddListener(() =>
            {
                UpgradePanel();
                buttons.SetActive(false);
            });
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                blockImage.enabled = false;
                upgradeButton.gameObject.SetActive(true);
                _upgradePanelTween.PlayBackwards();
                buttons.SetActive(true);
                BackendGameData.instance.GameDataUpdate();
            });
            dataDeleteButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                deletePanelBlockImage.enabled = true;
                _deletePanelTween.Restart();
            });
            yesButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                deletePanelBlockImage.enabled = false;
                _deletePanelTween.PlayBackwards();

                var allXp = _totalSpentXp + BackendGameData.userData.xp;

                BackendGameData.userData.xp = allXp;
                xpText.text = "XP " + allXp;
                _totalSpentXp = 0;

                for (var i = 0; i < towerButtons.childCount; i++)
                {
                    var levelCountText = towerButtons.GetChild(i).GetChild(0).GetComponent<TMP_Text>();
                    var towerCostText = towerButtons.GetChild(i).GetChild(1).GetComponent<TMP_Text>();
                    var towerUpgradeButton = towerButtons.GetChild(i).GetChild(2).GetComponent<TowerUpgradeButton>();
                    var towerButton = towerUpgradeButton.GetComponent<Button>();

                    towerUpgradeButton.UpgradeCount = 0;
                    towerButton.interactable = true;
                    towerCostText.text = "XP " + (towerUpgradeButton.UpgradeCount + 1) * 25;
                    levelCountText.text = "Lv 0 / " + towerMaxLevel;
                    towerData[i].InitState();
                }
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
            for (var i = 0; i < towerButtons.childCount; i++)
            {
                var towerButtonParent = towerButtons.GetChild(i);
                var levelCountText = towerButtonParent.GetChild(0).GetComponent<TMP_Text>();
                var towerCostText = towerButtonParent.GetChild(1).GetComponent<TMP_Text>();
                var towerUpgradeButton = towerButtonParent.GetChild(2).GetComponent<TowerUpgradeButton>();
                var towerButton = towerUpgradeButton.GetComponent<Button>();
                towerUpgradeButton.UpgradeCount =
                    (byte)BackendGameData.userData.towerLevelTable[towerData[i].towerType.ToString()];

                if (towerUpgradeButton.UpgradeCount < towerMaxLevel)
                {
                    towerCostText.text = "XP " + (towerUpgradeButton.UpgradeCount + 1) * 25;
                    levelCountText.text = "Lv " + towerUpgradeButton.UpgradeCount + " / " + towerMaxLevel;
                }
                else
                {
                    towerCostText.text = "";
                    levelCountText.text = "Lv " + towerMaxLevel + " / " + towerMaxLevel;
                    towerButton.interactable = false;
                }

                var index = i;
                towerButton.onClick.AddListener(() =>
                {
                    SoundManager.PlayUISound(SoundEnum.ButtonSound);
                    if (towerUpgradeButton.UpgradeCount >= towerMaxLevel ||
                        BackendGameData.userData.xp < (towerUpgradeButton.UpgradeCount + 1) * 25) return;
                    BackendGameData.userData.xp -= (towerUpgradeButton.UpgradeCount + 1) * 25;
                    _totalSpentXp += (towerUpgradeButton.UpgradeCount + 1) * 25;
                    BackendGameData.userData.totalSpentXp = _totalSpentXp;
                    xpText.text = "XP " + BackendGameData.userData.xp;
                    towerUpgradeButton.UpgradeCount++;
                    towerCostText.text = "XP " + (towerUpgradeButton.UpgradeCount + 1) * 25;
                    levelCountText.text = "Lv " + towerUpgradeButton.UpgradeCount + " / " + towerMaxLevel;
                    BackendGameData.userData.towerLevelTable[towerData[index].towerType.ToString()] =
                        towerUpgradeButton.UpgradeCount;
                    towerData[index].UpgradeData(towerData[index].towerType);

                    if (towerUpgradeButton.UpgradeCount >= towerMaxLevel)
                    {
                        towerCostText.text = "";
                        towerButton.interactable = false;
                    }
                });
            }
        }
    }
}