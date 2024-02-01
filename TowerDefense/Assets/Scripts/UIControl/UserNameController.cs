using System;
using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class UserNameController : MonoBehaviour
    {
        private UserRankController _userRankController;

        private string _curNickname;
        private bool _isChangedNickname;
        private Tween _nicknamePanelTween;
        private Sequence _notifyMinLengthTween;

        [SerializeField] private Button userIconButton;
        [SerializeField] private Image nicknameBlockImage;
        [SerializeField] private Button nicknameBlockButton;
        [SerializeField] private RectTransform nicknamePanel;
        [SerializeField] private TMP_InputField userNameField;
        [SerializeField] private RectTransform notifyMinLengthPanel;

        private void Awake()
        {
            _userRankController = FindAnyObjectByType<UserRankController>();
            userIconButton.onClick.AddListener(OpenUserNamePanel);
            _nicknamePanelTween = nicknamePanel.DOScaleX(1, 0.25f).From(0).SetEase(Ease.OutBack)
                .SetAutoKill(false).Pause();
            _notifyMinLengthTween = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(notifyMinLengthPanel.DOScaleX(1, 0.2f).From(0).SetEase(Ease.OutExpo))
                .Append(notifyMinLengthPanel.DOScaleX(0, 0.2f).SetEase(Ease.OutExpo).SetDelay(1));

            nicknameBlockImage.enabled = false;
            nicknameBlockButton.interactable = false;
            nicknameBlockButton.onClick.AddListener(CloseUserNamePanel);
            userNameField.text = BackendLogin.instance.GetUserNickName();
            userNameField.onSelect.AddListener(GetCurNickName);
            userNameField.onDeselect.AddListener(GoBackPrevNickName);
            userNameField.onSubmit.AddListener(SubmitNickName);
        }

        private void OnDestroy()
        {
            _nicknamePanelTween?.Kill();
            _notifyMinLengthTween?.Kill();
        }

        private void OpenUserNamePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _nicknamePanelTween.Restart();
            nicknameBlockImage.enabled = true;
            nicknameBlockButton.interactable = true;
        }

        private void CloseUserNamePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _nicknamePanelTween.PlayBackwards();
            nicknameBlockImage.enabled = false;
            nicknameBlockButton.interactable = false;
        }

        private void GetCurNickName(string arg0)
        {
            _isChangedNickname = false;
            _curNickname = userNameField.text;
        }

        private void GoBackPrevNickName(string arg0)
        {
            if (_isChangedNickname) return;
            Debug.Log("=================deselect input field=================");
            userNameField.text = _curNickname;
        }

        private void SubmitNickName(string arg0)
        {
            if (userNameField.text.Length > 0)
            {
                if (BackendLogin.instance.UpdateNickname(userNameField.text))
                {
                    _isChangedNickname = true;
                    Debug.Log("닉넴 변경성공");
                    _userRankController.SetRanking();
                }
                else
                {
                    _isChangedNickname = false;
                    Debug.Log("변경 실패");
                    userNameField.text = _curNickname;
                }
            }
            else
            {
                _notifyMinLengthTween.Restart();
            }
        }
    }
}