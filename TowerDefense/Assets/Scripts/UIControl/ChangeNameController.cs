using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using LobbyUIControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class ChangeNameController : MonoBehaviour
    {
        private LobbyUI _lobbyUI;
        private UserRankController _userRankController;
        private string _curNickname;
        private Sequence _changeNamePanelSequence;

        [SerializeField] private Button userIconButton;
        [SerializeField] private CanvasGroup changeNamePanelGroup;
        [SerializeField] private TMP_InputField userNameField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private void Awake()
        {
            _lobbyUI = GetComponentInParent<LobbyUI>();
            _userRankController = FindAnyObjectByType<UserRankController>();
            userIconButton.onClick.AddListener(OpenUserNamePanel);

            var namePanelRect = changeNamePanelGroup.GetComponent<RectTransform>();
            _changeNamePanelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(changeNamePanelGroup.DOFade(1, 0.25f).From(0))
                .Join(namePanelRect.DOAnchorPosX(0, 0.25f).From(new Vector2(-100, 0)));

            _changeNamePanelSequence.OnComplete(() => changeNamePanelGroup.blocksRaycasts = true);
            _changeNamePanelSequence.OnRewind(() => { _lobbyUI.OffBlockImage(); });
            changeNamePanelGroup.blocksRaycasts = false;
            userNameField.text = BackendLogin.instance.GetUserNickName();
            userNameField.onSelect.AddListener(GetCurNickName);

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
            _lobbyUI.OnBackgroundImage();
            _changeNamePanelSequence.Restart();
        }

        private void CloseUserNamePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _lobbyUI.OffBackgroundImage();
            changeNamePanelGroup.blocksRaycasts = false;
            _changeNamePanelSequence.PlayBackwards();
        }

        private void GetCurNickName(string arg0)
        {
            _curNickname = userNameField.text;
            TouchScreenKeyboard.Open(userNameField.text, TouchScreenKeyboardType.Default, false, false, false);
        }

        private void SubmitNickName()
        {
            if (userNameField.text.Length > 0)
            {
                userNameField.text = userNameField.text.Trim();
                var updateNickNameState = BackendLogin.instance.UpdateNickname(userNameField.text);
                if (updateNickNameState.Item1)
                {
                    _userRankController.SetRanking();
                    _lobbyUI.OffBackgroundImage();
                    _changeNamePanelSequence.PlayBackwards();
                    _lobbyUI.NoticeTween(NoticeTableEnum.SuccessChangedName);
                }
                else
                {
                    userNameField.text = _curNickname;
                    if (updateNickNameState.Item2 == "409")
                    {
                        _lobbyUI.NoticeTween(NoticeTableEnum.DuplicateName);
                    }
                }
            }
        }
    }
}