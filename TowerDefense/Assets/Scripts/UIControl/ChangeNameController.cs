using System;
using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIControl
{
    public class ChangeNameController : MonoBehaviour
    {
        private UserRankController _userRankController;

        private string _curNickname;
        private bool _isChangedNickname;
        private Sequence _changeNamePanelSequence;

        [SerializeField] private Button userIconButton;
        [SerializeField] private Image blockImage;
        [SerializeField] private CanvasGroup changeNamePanelGroup;
        [SerializeField] private TMP_InputField userNameField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private void Awake()
        {
            _userRankController = FindAnyObjectByType<UserRankController>();
            userIconButton.onClick.AddListener(OpenUserNamePanel);
            _changeNamePanelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(changeNamePanelGroup.DOFade(1, 0.25f).From(0))
                .Join(changeNamePanelGroup.GetComponent<RectTransform>().DOAnchorPosX(0, 0.25f)
                    .From(new Vector2(-100, 0)));

            blockImage.enabled = false;
            changeNamePanelGroup.blocksRaycasts = false;
            userNameField.text = BackendLogin.instance.GetUserNickName();
            userNameField.onSelect.AddListener(GetCurNickName);
            userNameField.onDeselect.AddListener(GoBackPrevNickName);

            confirmButton.onClick.AddListener(SubmitNickName);
            cancelButton.onClick.AddListener(CloseUserNamePanel);
        }

        private void OnDestroy()
        {
            _changeNamePanelSequence?.Kill();
        }

        private void OpenUserNamePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _changeNamePanelSequence.OnComplete(() => changeNamePanelGroup.blocksRaycasts = true).Restart();
            blockImage.enabled = true;
        }

        private void CloseUserNamePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _changeNamePanelSequence.OnRewind(() => changeNamePanelGroup.blocksRaycasts = false).PlayBackwards();
            blockImage.enabled = false;
        }

        private void GetCurNickName(string arg0)
        {
            _isChangedNickname = false;
            _curNickname = userNameField.text;
        }

        private void GoBackPrevNickName(string arg0)
        {
            if (_isChangedNickname) return;
            userNameField.text = _curNickname;
        }

        private void SubmitNickName()
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
        }
    }
}