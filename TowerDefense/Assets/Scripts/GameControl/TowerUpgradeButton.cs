using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameControl
{
    public class TowerUpgradeButton : MonoBehaviour
    {
        private TMP_Text _xpCostText;
        private TMP_Text _upgradeCountText;
        private Tween _upgradeTween;

        public byte UpgradeCount { get; set; }

        private void Awake()
        {
            _upgradeTween = transform.DOScale(1, 0.25f).From(0.7f).SetEase(Ease.OutBack).SetAutoKill(false)
                .SetUpdate(true);
            GetComponent<Button>().onClick.AddListener(() => { _upgradeTween.Restart(); });
        }
    }
}