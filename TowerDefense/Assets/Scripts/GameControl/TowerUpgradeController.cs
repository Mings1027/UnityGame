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
        [SerializeField] private TMP_Text xpText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform upgradePanel;
        [SerializeField] private Transform towerButtons;

        private void Start()
        {
            upgradePanel.DOScale(0, 0);
            DataManager.Xp = PlayerPrefs.GetInt(StringManager.Xp);
            xpText.text = "XP : " + DataManager.Xp;
            upgradeButton.onClick.AddListener(UpgradePanel);
            closeButton.onClick.AddListener(() =>
            {
                upgradeButton.gameObject.SetActive(true);
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                upgradePanel.DOScale(0, 0.25f).SetEase(Ease.InBack);
            });

            for (int i = 0; i < towerButtons.childCount; i++)
            {
                towerButtons.GetChild(i).GetComponent<TowerUpgradeButton>().OnSetXpTextEvent +=
                    () => xpText.text = "XP : " + DataManager.Xp;
            }
        }

        private void UpgradePanel()
        {
            upgradeButton.gameObject.SetActive(false);
            SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            upgradePanel.DOScale(1, 0.25f).SetEase(Ease.OutBack);
        }
    }
}