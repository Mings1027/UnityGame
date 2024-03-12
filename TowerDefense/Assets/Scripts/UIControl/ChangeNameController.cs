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
        [SerializeField] private TMP_Text curUserNickName;
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
            curUserNickName.text = BackendLogin.instance.GetUserNickName();
            userNameField.onSelect.AddListener(_ => TouchScreenKeyboard.Open(userNameField.text,
                TouchScreenKeyboardType.Default, false, false, false));

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
            userNameField.text = "";
            _lobbyUI.OnBackgroundImage();
            _lobbyUI.SetActiveButtons(false, false);
            _changeNamePanelSequence.Restart();
        }

        private void CloseUserNamePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _lobbyUI.OffBackgroundImage();
            _lobbyUI.SetActiveButtons(true, false);
            changeNamePanelGroup.blocksRaycasts = false;
            _changeNamePanelSequence.PlayBackwards();
        }

        private void SubmitNickName()
        {
            if (userNameField.text.Length > 0)
            {
                var updateNickNameState = BackendLogin.instance.UpdateNickname(userNameField.text);
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