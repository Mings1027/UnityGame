using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class LobbySetting : MonoBehaviour
    {
        private bool _isOpen;
        private Sequence _settingPanelSequence;
        [SerializeField] private Button settingButton;
        [SerializeField] private Toggle bgmToggle;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Transform settingPanel;

        private void Start()
        {
            _settingPanelSequence = DOTween.Sequence()
                .Append(settingPanel.GetChild(0).DOLocalMoveY(0, 0.5f).From().SetEase(Ease.OutBack))
                .Join(settingPanel.GetChild(1).DOLocalMoveY(0, 0.5f).From().SetEase(Ease.OutBack))
                .Join(settingPanel.GetChild(2).DOLocalMoveY(0, 0.5f).From().SetEase(Ease.OutBack))
                .SetAutoKill(false).Pause();

            settingButton.onClick.AddListener(() =>
            {
                settingButton.transform.DOScale(1, 0.25f).From(0.5f).SetEase(Ease.OutBack);
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
                SettingPanel();
            });
            bgmToggle.isOn = !SoundManager.Instance.BGMOn;
            bgmToggle.onValueChanged.AddListener(delegate
            {
                bgmToggle.transform.DOScale(1, 0.25f).From(0.5f).SetEase(Ease.OutBack);
                SoundManager.Instance.ToggleBGM(!bgmToggle.isOn);
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            });
            sfxToggle.isOn = !SoundManager.Instance.SfxOn;
            sfxToggle.onValueChanged.AddListener(delegate
            {
                sfxToggle.transform.DOScale(1, 0.25f).From(0.5f).SetEase(Ease.OutBack);
                SoundManager.Instance.ToggleSfx(!sfxToggle.isOn);
                SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            });
        }

        private void OnDestroy()
        {
            _settingPanelSequence?.Kill();
        }

        private void SettingPanel()
        {
            if (_isOpen)
            {
                _settingPanelSequence.PlayBackwards();
                _isOpen = false;
            }
            else
            {
                _settingPanelSequence.Restart();
                _isOpen = true;
            }
        }
    }
}