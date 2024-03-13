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
    public class UserInfoController : MonoBehaviour
    {
        private LobbyUI _lobbyUI;
        private UserRankController _userRankController;
        private string _curNickname;
        private Sequence _userinfoSequence;
        private Sequence _changeNamePanelSequence;

        [SerializeField] private Button userIconButton;
        [SerializeField] private Button closeUserInfoButton;
        [SerializeField] private Button changeNameButton;
        [SerializeField] private CanvasGroup userInfoGroup;
        [SerializeField] private CanvasGroup changeNamePanelGroup;
        [SerializeField] private TMP_Text curUserNickName;
        [SerializeField] private TMP_InputField userNameField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private void Awake()
        {
            _lobbyUI = GetComponentInParent<LobbyUI>();
            _userRankController = FindAnyObjectByType<UserRankController>();
            userIconButton.onClick.AddListener(OpenUserInfoPanel);
            closeUserInfoButton.onClick.AddListener(CloseUserInfoPanel);
            changeNameButton.onClick.AddListener(OpenChangeNamePanel);

            var userInfoRect = userInfoGroup.GetComponent<RectTransform>();
            _userinfoSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(userInfoGroup.DOFade(1, 0.25f).From(0))
                .Join(userInfoRect.DOAnchorPosX(0, 0.25f).From(new Vector2(-100, 0)));

            _userinfoSequence.OnComplete(() => userInfoGroup.blocksRaycasts = true);
            _userinfoSequence.OnRewind(() => _lobbyUI.OffBlockImage());
            userInfoGroup.blocksRaycasts = false;
            
            var namePanelRect = changeNamePanelGroup.GetComponent<RectTransform>();
            _changeNamePanelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(changeNamePanelGroup.DOFade(1, 0.25f).From(0))
                .Join(namePanelRect.DOAnchorPosX(0, 0.25f).From(new Vector2(-100, 0)));

            _changeNamePanelSequence.OnComplete(() => changeNamePanelGroup.blocksRaycasts = true);
            changeNamePanelGroup.blocksRaycasts = false;
            curUserNickName.text = BackendLogin.GetUserNickName();
            userNameField.onSelect.AddListener(_ => TouchScreenKeyboard.Open(userNameField.text,
                TouchScreenKeyboardType.Default, false, false, false));

            confirmButton.onClick.AddListener(SubmitNickName);
            cancelButton.onClick.AddListener(CloseChangeNamePanel);
        }

        private void OnDestroy()
        {
            _changeNamePanelSequence?.Kill();
        }

        private void OpenUserInfoPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            userNameField.text = "";
            _lobbyUI.OnBackgroundImage();
            _lobbyUI.SetActiveButtons(false, false);
            _userinfoSequence.Restart();
        }

        private void CloseUserInfoPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _lobbyUI.OffBackgroundImage();
            _lobbyUI.SetActiveButtons(true, false);
            userInfoGroup.blocksRaycasts = false;
            _userinfoSequence.PlayBackwards();
        }

        private void OpenChangeNamePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _changeNamePanelSequence.Restart();
        }

        private void CloseChangeNamePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            changeNamePanelGroup.blocksRaycasts = false;
            _changeNamePanelSequence.PlayBackwards();
        }

        private void SubmitNickName()
        {
            if (userNameField.text.Length > 0)
            {
                var updateNickNameState = BackendLogin.UpdateNickname(userNameField.text);
                if (updateNickNameState.Item1)
                {
                    curUserNickName.text = userNameField.text.Trim();
                    userNameField.text = "";
                    _userRankController.SetRanking();
                    _lobbyUI.OffBackgroundImage();
                    _lobbyUI.SetActiveButtons(true, false);
                    _lobbyUI.NoticeTween(FloatingNotifyEnum.SuccessChangedName);
                    _changeNamePanelSequence.PlayBackwards();
                }
                else
                {
                    if (updateNickNameState.Item2 == "409")
                    {
                        _lobbyUI.NoticeTween(FloatingNotifyEnum.DuplicateName);
                    }
                }
            }
            else
            {
                _lobbyUI.NoticeTween(FloatingNotifyEnum.AtLeastOneCharacter);
            }
        }
    }
}