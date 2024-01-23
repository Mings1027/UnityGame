using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyControl
{
    public class LobbySetting : MonoBehaviour
    {
        private bool _isOpen;
        private Sequence _settingPanelSequence;
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

            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                button.transform.DOScale(1, 0.25f).From(0.5f).SetEase(Ease.OutBack);
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                SettingPanel();
            });
            bgmToggle.isOn = !SoundManager.bgmOn;
            bgmToggle.onValueChanged.AddListener(delegate
            {
                bgmToggle.transform.DOScale(1, 0.25f).From(0.5f).SetEase(Ease.OutBack);
                SoundManager.ToggleBGM(!bgmToggle.isOn);
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
            });
            sfxToggle.isOn = !SoundManager.sfxOn;
            sfxToggle.onValueChanged.AddListener(delegate
            {
                sfxToggle.transform.DOScale(1, 0.25f).From(0.5f).SetEase(Ease.OutBack);
                SoundManager.ToggleSfx(!sfxToggle.isOn);
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
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