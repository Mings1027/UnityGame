using BackendControl;
using CustomEnumControl;
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
        [SerializeField] private Image blockImage;
        [SerializeField] private TMP_InputField userNameField;

        private void Awake()
        {
            _userIconButton = GetComponent<Button>();
            _userIconButton.onClick.AddListener(OnOffUserName);
            blockImage.enabled = false;
            userNameField.text = BackendLogin.instance.GetUserNickName();
            userNameField.gameObject.SetActive(false);
            userNameField.onSelect.AddListener(GetCurNickName);
            userNameField.onDeselect.AddListener(GoBackPrevNickName);
            userNameField.onSubmit.AddListener(SubmitNickName);
        }

        private void OnOffUserName()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_isOpenUserName)
            {
                _isOpenUserName = false;
                userNameField.gameObject.SetActive(false);
            }
            else
            {
                _isOpenUserName = true;
                userNameField.gameObject.SetActive(true);
            }
        }

        private void GetCurNickName(string arg0)
        {
            blockImage.enabled = true;
            _curNickName = userNameField.text;
        }

        private void GoBackPrevNickName(string arg0)
        {
            blockImage.enabled = false;
            userNameField.text = _curNickName;
        }

        private void SubmitNickName(string arg0)
        {
            blockImage.enabled = false;
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