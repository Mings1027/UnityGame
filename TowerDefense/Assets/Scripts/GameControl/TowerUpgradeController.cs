using CustomEnumControl;
using DataControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameControl
{
    public class TowerUpgradeController : MonoBehaviour
    {
        private Tween _upgradePanelTween;
        [SerializeField] private TMP_Text xpText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform upgradePanel;
        [SerializeField] private Transform towerButtons;
        [SerializeField] private Transform towerCostTexts;
        [SerializeField] private Transform levelCountTexts;
        [SerializeField] private BattleTowerData[] towerData;

        private void Awake()
        {
            _upgradePanelTween =
                upgradePanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            closeButton.gameObject.SetActive(false);
        }

        private void Start()
        {
            DataManager.Xp = PlayerPrefs.GetInt(StringManager.Xp);
            xpText.text = "XP : " + DataManager.Xp;
            upgradeButton.onClick.AddListener(UpgradePanel);
            closeButton.onClick.AddListener(() =>
            {
                upgradeButton.gameObject.SetActive(true);
                closeButton.gameObject.SetActive(false);
                _upgradePanelTween.PlayBackwards();
            });
            TowerUpgradeButtonInit();
        }

        private void OnDestroy()
        {
            _upgradePanelTween?.Kill();
        }

        private void UpgradePanel()
        {
            upgradeButton.gameObject.SetActive(false);
            SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            closeButton.gameObject.SetActive(true);
            _upgradePanelTween.Restart();
        }

        private void TowerUpgradeButtonInit()
        {
            for (var i = 0; i < towerButtons.childCount; i++)
            {
                var towerUpgradeButton = towerButtons.GetChild(i).GetComponent<TowerUpgradeButton>();
                var towerButton = towerUpgradeButton.GetComponent<Button>();
                var towerCostText = towerCostTexts.GetChild(i).GetComponent<TMP_Text>();
                var levelCountText = levelCountTexts.GetChild(i).GetComponent<TMP_Text>();
                towerUpgradeButton.UpgradeCount =
                    (byte)PlayerPrefs.GetInt(towerData[i].TowerType + StringManager.UpgradeCount);

                if (towerUpgradeButton.UpgradeCount < 10)
                {
                    towerCostText.text = "XP : " + (towerUpgradeButton.UpgradeCount + 1) * 25;
                    levelCountText.text = towerUpgradeButton.UpgradeCount + " / 10";
                }
                else
                {
                    towerCostText.text = "";
                    levelCountText.text = "10 / 10";
                    towerButton.interactable = false;
                }

                var index = i;
                towerButton.onClick.AddListener(() =>
                {
                    SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                    if (towerUpgradeButton.UpgradeCount >= 10 ||
                        DataManager.Xp < (towerUpgradeButton.UpgradeCount + 1) * 25) return;
                    DataManager.Xp -= (towerUpgradeButton.UpgradeCount + 1) * 25;
                    xpText.text = "XP : " + DataManager.Xp;
                    towerUpgradeButton.UpgradeCount++;
                    towerCostText.text = "XP : " + (towerUpgradeButton.UpgradeCount + 1) * 25;
                    levelCountText.text = towerUpgradeButton.UpgradeCount + " / 10";
                    PlayerPrefs.SetInt(StringManager.Xp, DataManager.Xp);
                    PlayerPrefs.SetInt(towerData[index].TowerType + StringManager.UpgradeCount,
                        towerUpgradeButton.UpgradeCount);
                    towerData[index].UpgradeData(towerData[index].TowerType);

                    if (towerUpgradeButton.UpgradeCount >= 10)
                    {
                        towerCostText.text = "";
                        towerButton.interactable = false;
                    }
                });
            }
        }
    }
}