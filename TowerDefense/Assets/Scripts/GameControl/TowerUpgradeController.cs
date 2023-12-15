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
        private Tween _deletePanelTween;

        [SerializeField] private TMP_Text xpText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button dataDeleteButton;
        [SerializeField] private Transform deletePanel;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private Transform upgradePanel;
        [SerializeField] private Transform towerButtons;
        [SerializeField] private Transform towerCostTexts;
        [SerializeField] private Transform levelCountTexts;
        [SerializeField] private BattleTowerData[] towerData;

        private void Awake()
        {
            _upgradePanelTween =
                upgradePanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            _deletePanelTween = deletePanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause()
                .SetUpdate(true);
            closeButton.gameObject.SetActive(false);
            Input.multiTouchEnabled = false;

            ButtonInit();
        }

        private void Start()
        {
            DataManager.Xp = PlayerPrefs.GetInt(StringManager.Xp);
            xpText.text = "XP : " + DataManager.Xp;
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
            SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            closeButton.gameObject.SetActive(true);
            _upgradePanelTween.Restart();
        }

        private void ButtonInit()
        {
            upgradeButton.onClick.AddListener(UpgradePanel);
            closeButton.onClick.AddListener(() =>
            {
                upgradeButton.gameObject.SetActive(true);
                closeButton.gameObject.SetActive(false);
                _upgradePanelTween.PlayBackwards();
            });
            dataDeleteButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                _deletePanelTween.Restart();
            });
            yesButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                _deletePanelTween.PlayBackwards();
                PlayerPrefs.DeleteAll();
                DataManager.Xp = 0;
                xpText.text = "XP : 0";
                for (var i = 0; i < towerButtons.childCount; i++)
                {
                    var towerUpgradeButton = towerButtons.GetChild(i).GetComponent<TowerUpgradeButton>();
                    var towerCostText = towerCostTexts.GetChild(i).GetComponent<TMP_Text>();
                    var levelCountText = levelCountTexts.GetChild(i).GetComponent<TMP_Text>();
                    var towerButton = towerUpgradeButton.GetComponent<Button>();

                    towerUpgradeButton.UpgradeCount = 0;
                    towerButton.interactable = true;
                    towerCostText.text = "XP : " + (towerUpgradeButton.UpgradeCount + 1) * 25;
                    levelCountText.text = "0 / 10";
                    towerData[i].InitState();
                }
            });
            noButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                _deletePanelTween.PlayBackwards();
            });
        }

        private void TowerUpgradeButtonInit()
        {
            for (var i = 0; i < towerButtons.childCount; i++)
            {
                var towerButtonParent = towerButtons.GetChild(i);
                var towerCostText = towerButtonParent.GetChild(0).GetComponent<TMP_Text>();
                var towerUpgradeButton = towerButtonParent.GetChild(1).GetComponent<TowerUpgradeButton>();
                var towerButton = towerUpgradeButton.GetComponent<Button>();
                var levelCountText = towerButtonParent.GetChild(2).GetComponent<TMP_Text>();
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