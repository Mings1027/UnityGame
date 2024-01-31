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
        private string _curNickName;
        private bool _isOpenUserName;
        private Button _userIconButton;
        private Tween _inputFieldTween;
        
        [SerializeField] private TMP_InputField userNameField;

        private void Awake()
        {
            _userIconButton = GetComponent<Button>();
            _userIconButton.onClick.AddListener(OnOffUserName);
            _inputFieldTween = userNameField.transform.DOScaleX(1, 0.25f).From(0).SetEase(Ease.OutBack)
                .SetAutoKill(false).Pause();
            userNameField.text = BackendLogin.instance.GetUserNickName();
            userNameField.onSelect.AddListener(GetCurNickName);
            userNameField.onDeselect.AddListener(GoBackPrevNickName);
            userNameField.onSubmit.AddListener(SubmitNickName);
        }

        private void OnDisable()
        {
            _inputFieldTween?.Kill();
        }

        private void OnOffUserName()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_isOpenUserName)
            {
                _isOpenUserName = false;
                _inputFieldTween.PlayBackwards();
            }
            else
            {
                _isOpenUserName = true;
                _inputFieldTween.Restart();
            }
        }

        private void GetCurNickName(string arg0)
        {
            _curNickName = userNameField.text;
        }

        private void GoBackPrevNickName(string arg0)
        {
            userNameField.text = _curNickName;
        }

        private void SubmitNickName(string arg0)
        {
            if (userNameField.text.Length > 0)
            {
                BackendLogin.instance.UpdateNickname(userNameField.text);
            }
            else
            {
                Debug.Log("바꿀 닉네임을 적어주세요");
            }
        }
    }
}